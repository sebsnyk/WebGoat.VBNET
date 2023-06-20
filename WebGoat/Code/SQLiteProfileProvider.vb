Imports System
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Configuration
Imports System.Configuration.Provider
Imports System.Data
Imports Mono.Data.Sqlite
Imports System.Globalization
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports System.Web.Profile
Imports System.Xml.Serialization
Imports System.Runtime.InteropServices

Namespace TechInfoSystems.Data.SQLite
    Public NotInheritable Class SQLiteProfileProvider
        Inherits ProfileProvider

        Private Shared _connectionString As String
        Private Const HTTP_TRANSACTION_ID As String = "SQLiteTran"
        Private Const USER_TB_NAME As String = "[aspnet_Users]"
        Private Const PROFILE_TB_NAME As String = "[aspnet_Profile]"
        Private Const APP_TB_NAME As String = "[aspnet_Applications]"
        Private Const MAX_APPLICATION_NAME_LENGTH As Integer = 256
        Private Shared _applicationId As String
        Private Shared _applicationName As String
        Private Shared _membershipApplicationId As String
        Private Shared _membershipApplicationName As String

        Public Overrides Property ApplicationName As String
            Get
                Return _applicationName
            End Get
            Set(ByVal value As String)
                If value.Length > MAX_APPLICATION_NAME_LENGTH Then Throw New ProviderException(String.Format("SQLiteProfileProvider error: applicationName must be less than or equal to {0} characters.", MAX_APPLICATION_NAME_LENGTH))
                _applicationName = value
                _applicationId = GetApplicationId(_applicationName)
            End Set
        End Property

        Public Shared Property MembershipApplicationName As String
            Get
                Return _membershipApplicationName
            End Get
            Set(ByVal value As String)
                If value.Length > MAX_APPLICATION_NAME_LENGTH Then Throw New ProviderException(String.Format("SQLiteProfileProvider error: membershipApplicationName must be less than or equal to {0} characters.", MAX_APPLICATION_NAME_LENGTH))
                _membershipApplicationName = value
                _membershipApplicationId = GetApplicationId(_membershipApplicationName)
            End Set
        End Property

        Public Overrides Sub Initialize(ByVal name As String, ByVal config As NameValueCollection)
            If config Is Nothing Then Throw New ArgumentNullException("config")
            If String.IsNullOrEmpty(name) Then name = "SQLiteProfileProvider"

            If String.IsNullOrEmpty(config("description")) Then
                config.Remove("description")
                config.Add("description", "SQLite Profile Provider")
            End If

            MyBase.Initialize(name, config)
            Dim connectionStringSettings As ConnectionStringSettings = ConfigurationManager.ConnectionStrings(config("connectionStringName"))

            If connectionStringSettings Is Nothing OrElse String.IsNullOrEmpty(connectionStringSettings.ConnectionString) Then
                Throw New ProviderException("Connection String is empty for SQLiteProfileProvider. Check the web configuration file (web.config).")
            End If

            _connectionString = connectionStringSettings.ConnectionString

            If config("applicationName") Is Nothing OrElse config("applicationName").Trim() = "" Then
                ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
            Else
                ApplicationName = config("applicationName")
            End If

            If config("membershipApplicationName") Is Nothing OrElse config("membershipApplicationName").Trim() = "" Then
                MembershipApplicationName = _applicationName
            Else
                MembershipApplicationName = config("membershipApplicationName")
            End If

            config.Remove("connectionStringName")
            config.Remove("applicationName")
            config.Remove("membershipApplicationName")

            If config.Count > 0 Then
                Dim attribUnrecognized As String = config.GetKey(0)
                If Not String.IsNullOrEmpty(attribUnrecognized) Then Throw New ProviderException("Unrecognized attribute: " & attribUnrecognized)
            End If

            VerifyApplication()
        End Sub

        Public Overrides Function GetPropertyValues(ByVal sc As SettingsContext, ByVal properties As SettingsPropertyCollection) As SettingsPropertyValueCollection
            Dim svc As SettingsPropertyValueCollection = New SettingsPropertyValueCollection()
            If properties.Count < 1 Then Return svc
            Dim username As String = CStr(sc("UserName"))

            For Each prop As SettingsProperty In properties

                If prop.SerializeAs = SettingsSerializeAs.ProviderSpecific Then

                    If prop.PropertyType.IsPrimitive OrElse prop.PropertyType = GetType(String) Then
                        prop.SerializeAs = SettingsSerializeAs.String
                    Else
                        prop.SerializeAs = SettingsSerializeAs.Xml
                    End If
                End If

                svc.Add(New SettingsPropertyValue(prop))
            Next

            If Not String.IsNullOrEmpty(username) Then
                GetPropertyValuesFromDatabase(username, svc)
            End If

            Return svc
        End Function

        Public Overrides Sub SetPropertyValues(ByVal sc As SettingsContext, ByVal properties As SettingsPropertyValueCollection)
            Dim username As String = CStr(sc("UserName"))
            Dim userIsAuthenticated As Boolean = CBool(sc("IsAuthenticated"))
            If String.IsNullOrEmpty(username) OrElse properties.Count < 1 Then Return
            Dim names As String = String.Empty
            Dim values As String = String.Empty
            Dim buf As Byte() = Nothing
            PrepareDataForSaving(names, values, buf, True, properties, userIsAuthenticated)
            If names.Length = 0 Then Return
            Dim tran As SqliteTransaction = Nothing
            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try
                If cn.State = ConnectionState.Closed Then cn.Open()
                If Not IsTransactionInProgress() Then tran = cn.BeginTransaction()

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT UserId FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId;"
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _membershipApplicationId)
                    Dim userId As String = TryCast(cmd.ExecuteScalar(), String)
                    If (userId Is Nothing) AndAlso (userIsAuthenticated) Then Return

                    If userId Is Nothing Then
                        userId = Guid.NewGuid().ToString()
                        CreateAnonymousUser(username, cn, tran, userId)
                    End If

                    cmd.CommandText = "SELECT COUNT(*) FROM " & PROFILE_TB_NAME & " WHERE UserId = $UserId"
                    cmd.Parameters.Clear()
                    cmd.Parameters.AddWithValue("$UserId", userId)

                    If Convert.ToInt64(cmd.ExecuteScalar()) > 0 Then
                        cmd.CommandText = "UPDATE " & PROFILE_TB_NAME & " SET PropertyNames = $PropertyNames, PropertyValuesString = $PropertyValuesString, PropertyValuesBinary = $PropertyValuesBinary, LastUpdatedDate = $LastUpdatedDate WHERE UserId = $UserId"
                    Else
                        cmd.CommandText = "INSERT INTO " & PROFILE_TB_NAME & " (UserId, PropertyNames, PropertyValuesString, PropertyValuesBinary, LastUpdatedDate) VALUES ($UserId, $PropertyNames, $PropertyValuesString, $PropertyValuesBinary, $LastUpdatedDate)"
                    End If

                    cmd.Parameters.Clear()
                    cmd.Parameters.AddWithValue("$UserId", userId)
                    cmd.Parameters.AddWithValue("$PropertyNames", names)
                    cmd.Parameters.AddWithValue("$PropertyValuesString", values)
                    cmd.Parameters.AddWithValue("$PropertyValuesBinary", buf)
                    cmd.Parameters.AddWithValue("$LastUpdatedDate", DateTime.UtcNow)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET LastActivityDate = $LastActivityDate WHERE UserId = $UserId"
                    cmd.Parameters.Clear()
                    cmd.Parameters.AddWithValue("$LastActivityDate", DateTime.UtcNow)
                    cmd.Parameters.AddWithValue("$UserId", userId)
                    cmd.ExecuteNonQuery()
                    If tran IsNot Nothing Then tran.Commit()
                End Using

            Catch

                If tran IsNot Nothing Then

                    Try
                        tran.Rollback()
                    Catch __unusedSqliteException1__ As SqliteException
                    End Try
                End If

                Throw
            Finally
                If tran IsNot Nothing Then tran.Dispose()
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Sub

        Public Overrides Function DeleteProfiles(ByVal profiles As ProfileInfoCollection) As Integer
            If profiles Is Nothing Then Throw New ArgumentNullException("profiles")
            If profiles.Count < 1 Then Throw New ArgumentException("Profiles collection is empty", "profiles")
            Dim numDeleted As Integer = 0
            Dim tran As SqliteTransaction = Nothing
            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try
                If cn.State = ConnectionState.Closed Then cn.Open()
                If Not IsTransactionInProgress() Then tran = cn.BeginTransaction()

                For Each profile As ProfileInfo In profiles
                    If DeleteProfile(cn, tran, profile.UserName.Trim()) Then numDeleted += 1
                Next

                If tran IsNot Nothing Then tran.Commit()
            Catch

                If tran IsNot Nothing Then

                    Try
                        tran.Rollback()
                    Catch __unusedSqliteException1__ As SqliteException
                    End Try
                End If

                Throw
            Finally
                If tran IsNot Nothing Then tran.Dispose()
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try

            Return numDeleted
        End Function

        Public Overrides Function DeleteProfiles(ByVal usernames As String()) As Integer
            Dim numDeleted As Integer = 0
            Dim tran As SqliteTransaction = Nothing
            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try
                If cn.State = ConnectionState.Closed Then cn.Open()
                If Not IsTransactionInProgress() Then tran = cn.BeginTransaction()

                For Each username As String In usernames
                    If DeleteProfile(cn, tran, username) Then numDeleted += 1
                Next

                If tran IsNot Nothing Then tran.Commit()
            Catch

                If tran IsNot Nothing Then

                    Try
                        tran.Rollback()
                    Catch __unusedSqliteException1__ As SqliteException
                    End Try
                End If

                Throw
            Finally
                If tran IsNot Nothing Then tran.Dispose()
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try

            Return numDeleted
        End Function

        Public Overrides Function DeleteInactiveProfiles(ByVal authenticationOption As ProfileAuthenticationOption, ByVal userInactiveSinceDate As DateTime) As Integer
            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "DELETE FROM " & PROFILE_TB_NAME & " WHERE UserId IN (SELECT UserId FROM " & USER_TB_NAME & " WHERE ApplicationId = $ApplicationId AND LastActivityDate <= $LastActivityDate" & GetClauseForAuthenticationOptions(authenticationOption) & ")"
                    cmd.Parameters.AddWithValue("$ApplicationId", _membershipApplicationId)
                    cmd.Parameters.AddWithValue("$LastActivityDate", userInactiveSinceDate)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Return cmd.ExecuteNonQuery()
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetNumberOfInactiveProfiles(ByVal authenticationOption As ProfileAuthenticationOption, ByVal userInactiveSinceDate As DateTime) As Integer
            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT COUNT(*) FROM " & USER_TB_NAME & " u, " & PROFILE_TB_NAME & " p " & "WHERE u.ApplicationId = $ApplicationId AND u.LastActivityDate <= $LastActivityDate AND u.UserId = p.UserId" & GetClauseForAuthenticationOptions(authenticationOption)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    cmd.Parameters.AddWithValue("$ApplicationId", _membershipApplicationId)
                    cmd.Parameters.AddWithValue("$LastActivityDate", userInactiveSinceDate)
                    Return cmd.ExecuteNonQuery()
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetAllProfiles(ByVal authenticationOption As ProfileAuthenticationOption, ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As ProfileInfoCollection
            Dim sqlQuery As String = "SELECT u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate, length(p.PropertyNames) + length(p.PropertyValuesString) FROM " & USER_TB_NAME & " u, " & PROFILE_TB_NAME & " p WHERE u.ApplicationId = $ApplicationId AND u.UserId = p.UserId " & GetClauseForAuthenticationOptions(authenticationOption)
            Dim prm As SqliteParameter = New SqliteParameter("$ApplicationId", DbType.String, 36)
            prm.Value = _membershipApplicationId
            Dim args As SqliteParameter() = New SqliteParameter(0) {}
            args(0) = prm
            Return GetProfilesForQuery(sqlQuery, args, pageIndex, pageSize, totalRecords)
        End Function

        Public Overrides Function GetAllInactiveProfiles(ByVal authenticationOption As ProfileAuthenticationOption, ByVal userInactiveSinceDate As DateTime, ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As ProfileInfoCollection
            Dim sqlQuery As String = "SELECT u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate, length(p.PropertyNames) + length(p.PropertyValuesString) FROM " & USER_TB_NAME & " u, " & PROFILE_TB_NAME & " p WHERE u.ApplicationId = $ApplicationId AND u.UserId = p.UserId AND u.LastActivityDate <= $LastActivityDate" & GetClauseForAuthenticationOptions(authenticationOption)
            Dim prm1 As SqliteParameter = New SqliteParameter("$ApplicationId", DbType.String, 256)
            prm1.Value = _membershipApplicationId
            Dim prm2 As SqliteParameter = New SqliteParameter("$LastActivityDate", DbType.DateTime)
            prm2.Value = userInactiveSinceDate
            Dim args As SqliteParameter() = New SqliteParameter(1) {}
            args(0) = prm1
            args(1) = prm2
            Return GetProfilesForQuery(sqlQuery, args, pageIndex, pageSize, totalRecords)
        End Function

        Public Overrides Function FindProfilesByUserName(ByVal authenticationOption As ProfileAuthenticationOption, ByVal usernameToMatch As String, ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As ProfileInfoCollection
            Dim sqlQuery As String = "SELECT u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate, length(p.PropertyNames) + length(p.PropertyValuesString) FROM " & USER_TB_NAME & " u, " & PROFILE_TB_NAME & " p WHERE u.ApplicationId = $ApplicationId AND u.UserId = p.UserId AND u.LoweredUserName LIKE $UserName" & GetClauseForAuthenticationOptions(authenticationOption)
            Dim prm1 As SqliteParameter = New SqliteParameter("$ApplicationId", DbType.String, 256)
            prm1.Value = _membershipApplicationId
            Dim prm2 As SqliteParameter = New SqliteParameter("$UserName", DbType.String, 256)
            prm2.Value = usernameToMatch.ToLowerInvariant()
            Dim args As SqliteParameter() = New SqliteParameter(1) {}
            args(0) = prm1
            args(1) = prm2
            Return GetProfilesForQuery(sqlQuery, args, pageIndex, pageSize, totalRecords)
        End Function

        Public Overrides Function FindInactiveProfilesByUserName(ByVal authenticationOption As ProfileAuthenticationOption, ByVal usernameToMatch As String, ByVal userInactiveSinceDate As DateTime, ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As ProfileInfoCollection
            Dim sqlQuery As String = "SELECT u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate, length(p.PropertyNames) + length(p.PropertyValuesString) FROM " & USER_TB_NAME & " u, " & PROFILE_TB_NAME & " p WHERE u.ApplicationId = $ApplicationId AND u.UserId = p.UserId AND u.UserName LIKE $UserName AND u.LastActivityDate <= $LastActivityDate" & GetClauseForAuthenticationOptions(authenticationOption)
            Dim prm1 As SqliteParameter = New SqliteParameter("$ApplicationId", DbType.String, 256)
            prm1.Value = _membershipApplicationId
            Dim prm2 As SqliteParameter = New SqliteParameter("$UserName", DbType.String, 256)
            prm2.Value = usernameToMatch.ToLowerInvariant()
            Dim prm3 As SqliteParameter = New SqliteParameter("$LastActivityDate", DbType.DateTime)
            prm3.Value = userInactiveSinceDate
            Dim args As SqliteParameter() = New SqliteParameter(2) {}
            args(0) = prm1
            args(1) = prm2
            args(2) = prm3
            Return GetProfilesForQuery(sqlQuery, args, pageIndex, pageSize, totalRecords)
        End Function

        Private Shared Sub CreateAnonymousUser(ByVal username As String, ByVal cn As SqliteConnection, ByVal tran As SqliteTransaction, ByVal userId As String)
            Using cmd As SqliteCommand = cn.CreateCommand()
                cmd.CommandText = "INSERT INTO " & USER_TB_NAME & " (UserId, Username, LoweredUsername, ApplicationId, Email, LoweredEmail, Comment, Password," & " PasswordFormat, PasswordSalt, PasswordQuestion," & " PasswordAnswer, IsApproved, IsAnonymous," & " CreateDate, LastPasswordChangedDate, LastActivityDate," & " LastLoginDate, IsLockedOut, LastLockoutDate," & " FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart," & " FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart)" & " Values($UserId, $Username, $LoweredUsername, $ApplicationId, $Email, $LoweredEmail, $Comment, $Password," & " $PasswordFormat, $PasswordSalt, $PasswordQuestion, $PasswordAnswer, $IsApproved, $IsAnonymous, $CreateDate, $LastPasswordChangedDate," & " $LastActivityDate, $LastLoginDate, $IsLockedOut, $LastLockoutDate," & " $FailedPasswordAttemptCount, $FailedPasswordAttemptWindowStart," & " $FailedPasswordAnswerAttemptCount, $FailedPasswordAnswerAttemptWindowStart)"
                cmd.Transaction = tran
                Dim nullDate As DateTime = DateTime.MinValue
                Dim nowDate As DateTime = DateTime.UtcNow
                cmd.Parameters.Add("$UserId", DbType.String).Value = userId
                cmd.Parameters.Add("$Username", DbType.String, 256).Value = username
                cmd.Parameters.Add("$LoweredUsername", DbType.String, 256).Value = username.ToLowerInvariant()
                cmd.Parameters.Add("$ApplicationId", DbType.String, 256).Value = _membershipApplicationId
                cmd.Parameters.Add("$Email", DbType.String, 256).Value = String.Empty
                cmd.Parameters.Add("$LoweredEmail", DbType.String, 256).Value = String.Empty
                cmd.Parameters.Add("$Comment", DbType.String, 3000).Value = Nothing
                cmd.Parameters.Add("$Password", DbType.String, 128).Value = Guid.NewGuid().ToString()
                cmd.Parameters.Add("$PasswordFormat", DbType.String, 128).Value = System.Web.Security.Membership.Provider.PasswordFormat.ToString()
                cmd.Parameters.Add("$PasswordSalt", DbType.String, 128).Value = String.Empty
                cmd.Parameters.Add("$PasswordQuestion", DbType.String, 256).Value = Nothing
                cmd.Parameters.Add("$PasswordAnswer", DbType.String, 128).Value = Nothing
                cmd.Parameters.Add("$IsApproved", DbType.Boolean).Value = True
                cmd.Parameters.Add("$IsAnonymous", DbType.Boolean).Value = True
                cmd.Parameters.Add("$CreateDate", DbType.DateTime).Value = nowDate
                cmd.Parameters.Add("$LastPasswordChangedDate", DbType.DateTime).Value = nullDate
                cmd.Parameters.Add("$LastActivityDate", DbType.DateTime).Value = nowDate
                cmd.Parameters.Add("$LastLoginDate", DbType.DateTime).Value = nullDate
                cmd.Parameters.Add("$IsLockedOut", DbType.Boolean).Value = False
                cmd.Parameters.Add("$LastLockoutDate", DbType.DateTime).Value = nullDate
                cmd.Parameters.Add("$FailedPasswordAttemptCount", DbType.Int32).Value = 0
                cmd.Parameters.Add("$FailedPasswordAttemptWindowStart", DbType.DateTime).Value = nullDate
                cmd.Parameters.Add("$FailedPasswordAnswerAttemptCount", DbType.Int32).Value = 0
                cmd.Parameters.Add("$FailedPasswordAnswerAttemptWindowStart", DbType.DateTime).Value = nullDate
                If cn.State <> ConnectionState.Open Then cn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Shared Sub ParseDataFromDb(ByVal names As String(), ByVal values As String, ByVal buf As Byte(), ByVal properties As SettingsPropertyValueCollection)
            If names Is Nothing OrElse values Is Nothing OrElse buf Is Nothing OrElse properties Is Nothing Then Return

            For iter As Integer = 0 To names.Length / 4 - 1
                Dim name As String = names(iter * 4)
                Dim pp As SettingsPropertyValue = properties(name)
                If pp Is Nothing Then Continue For
                Dim startPos As Integer = Int32.Parse(names(iter * 4 + 2), CultureInfo.InvariantCulture)
                Dim length As Integer = Int32.Parse(names(iter * 4 + 3), CultureInfo.InvariantCulture)

                If length = -1 AndAlso Not pp.[Property].PropertyType.IsValueType Then
                    pp.PropertyValue = Nothing
                    pp.IsDirty = False
                    pp.Deserialized = True
                End If

                If names(iter * 4 + 1) = "S" AndAlso startPos >= 0 AndAlso length > 0 AndAlso values.Length >= startPos + length Then
                    pp.PropertyValue = Deserialize(pp, values.Substring(startPos, length))
                End If

                If names(iter * 4 + 1) = "B" AndAlso startPos >= 0 AndAlso length > 0 AndAlso buf.Length >= startPos + length Then
                    Dim buf2 As Byte() = New Byte(length - 1) {}
                    Buffer.BlockCopy(buf, startPos, buf2, 0, length)
                    pp.PropertyValue = Deserialize(pp, buf2)
                End If
            Next
        End Sub

        Private Shared Sub GetPropertyValuesFromDatabase(ByVal username As String, ByVal svc As SettingsPropertyValueCollection)
            Dim names As String() = Nothing
            Dim values As String = Nothing
            Dim buffer As Byte() = Nothing
            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT UserId FROM " & USER_TB_NAME & " WHERE LoweredUsername = $UserName AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$UserName", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _membershipApplicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Dim userId As String = TryCast(cmd.ExecuteScalar(), String)

                    If userId IsNot Nothing Then
                        cmd.CommandText = "SELECT PropertyNames, PropertyValuesString, PropertyValuesBinary FROM " & PROFILE_TB_NAME & " WHERE UserId = $UserId"
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("$UserId", userId)

                        Using dr As SqliteDataReader = cmd.ExecuteReader()

                            If dr.Read() Then
                                names = dr.GetString(0).Split(":"c)
                                values = dr.GetString(1)
                                Dim length As Integer = CInt(dr.GetBytes(2, 0L, Nothing, 0, 0))
                                buffer = New Byte(length - 1) {}
                                dr.GetBytes(2, 0L, buffer, 0, length)
                            End If
                        End Using

                        cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET LastActivityDate = $LastActivityDate WHERE UserId = $UserId"
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("$LastActivityDate", DateTime.UtcNow)
                        cmd.Parameters.AddWithValue("$UserId", userId)
                        cmd.ExecuteNonQuery()
                    End If
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try

            If names IsNot Nothing AndAlso names.Length > 0 Then
                ParseDataFromDb(names, values, buffer, svc)
            End If
        End Sub

        Private Shared Function GetApplicationId(ByVal appName As String) As String
            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT ApplicationId FROM aspnet_Applications WHERE ApplicationName = $AppName"
                    cmd.Parameters.AddWithValue("$AppName", appName)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Return TryCast(cmd.ExecuteScalar(), String)
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Private Shared Sub VerifyApplication()
            If String.IsNullOrEmpty(_applicationId) OrElse String.IsNullOrEmpty(_membershipApplicationName) Then
                Dim cn As SqliteConnection = GetDbConnectionForProfile()

                Try

                    Using cmd As SqliteCommand = cn.CreateCommand()
                        cmd.CommandText = "INSERT INTO " & APP_TB_NAME & " (ApplicationId, ApplicationName, Description) VALUES ($ApplicationId, $ApplicationName, $Description)"
                        Dim profileApplicationId As String = Guid.NewGuid().ToString()
                        cmd.Parameters.AddWithValue("$ApplicationId", profileApplicationId)
                        cmd.Parameters.AddWithValue("$ApplicationName", _applicationName)
                        cmd.Parameters.AddWithValue("$Description", String.Empty)
                        If cn.State = ConnectionState.Closed Then cn.Open()

                        If String.IsNullOrEmpty(_applicationId) Then
                            cmd.ExecuteNonQuery()
                            _applicationId = profileApplicationId
                        End If

                        If (_applicationName <> _membershipApplicationName) AndAlso (String.IsNullOrEmpty(_membershipApplicationId)) Then
                            _membershipApplicationId = Guid.NewGuid().ToString()
                            cmd.Parameters("$ApplicationId").Value = _membershipApplicationId
                            cmd.Parameters("$ApplicationName").Value = _membershipApplicationName
                            cmd.ExecuteNonQuery()
                        End If
                    End Using

                Finally
                    If Not IsTransactionInProgress() Then cn.Dispose()
                End Try
            End If
        End Sub

        Private Shared Function GetProfilesForQuery(ByVal sqlQuery As String, ByVal args As SqliteParameter(), ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As ProfileInfoCollection
            If pageIndex < 0 Then Throw New ArgumentException("Page index must be non-negative", "pageIndex")
            If pageSize < 1 Then Throw New ArgumentException("Page size must be positive", "pageSize")
            Dim lBound As Long = CLng(pageIndex) * pageSize
            Dim uBound As Long = lBound + pageSize - 1

            If uBound > Int32.MaxValue Then
                Throw New ArgumentException("pageIndex*pageSize too large")
            End If

            Dim cn As SqliteConnection = GetDbConnectionForProfile()

            Try
                Dim profiles As ProfileInfoCollection = New ProfileInfoCollection()

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = sqlQuery

                    For iter As Integer = 0 To args.Length - 1
                        cmd.Parameters.Add(args(iter))
                    Next

                    If cn.State = ConnectionState.Closed Then cn.Open()

                    Using dr As SqliteDataReader = cmd.ExecuteReader()
                        totalRecords = 0

                        While dr.Read()
                            totalRecords += 1
                            If (totalRecords - 1 < lBound) OrElse (totalRecords - 1 > uBound) Then Continue While
                            Dim username As String = dr.GetString(0)
                            Dim isAnon As Boolean = dr.GetBoolean(1)
                            Dim dtLastActivity As DateTime = dr.GetDateTime(2)
                            Dim dtLastUpdated As DateTime = dr.GetDateTime(3)
                            Dim size As Integer = dr.GetInt32(4)
                            profiles.Add(New ProfileInfo(username, isAnon, dtLastActivity, dtLastUpdated, size))
                        End While

                        Return profiles
                    End Using
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Private Shared Function DeleteProfile(ByVal cn As SqliteConnection, ByVal tran As SqliteTransaction, ByVal username As String) As Boolean
            Dim deleteSuccessful As Boolean = False
            If cn.State <> ConnectionState.Open Then cn.Open()

            Using cmd As SqliteCommand = cn.CreateCommand()
                cmd.CommandText = "SELECT UserId FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                cmd.Parameters.AddWithValue("$ApplicationId", _membershipApplicationId)
                If tran IsNot Nothing Then cmd.Transaction = tran
                Dim userId As String = TryCast(cmd.ExecuteScalar(), String)

                If userId IsNot Nothing Then
                    cmd.CommandText = "DELETE FROM " & PROFILE_TB_NAME & " WHERE UserId = $UserId"
                    cmd.Parameters.Clear()
                    cmd.Parameters.Add("$UserId", DbType.String, 36).Value = userId
                    deleteSuccessful = (cmd.ExecuteNonQuery() <> 0)
                End If

                Return (deleteSuccessful)
            End Using
        End Function

        Private Shared Function Deserialize(ByVal prop As SettingsPropertyValue, ByVal obj As Object) As Object
            Dim val As Object = Nothing

            If obj IsNot Nothing Then

                If TypeOf obj Is String Then
                    val = GetObjectFromString(prop.[Property].PropertyType, prop.[Property].SerializeAs, CStr(obj))
                Else
                    Dim ms As MemoryStream = New MemoryStream(CType(obj, Byte()))

                    Try
                        val = (New BinaryFormatter()).Deserialize(ms)
                    Finally
                        ms.Close()
                    End Try
                End If

                If val IsNot Nothing AndAlso Not prop.[Property].PropertyType.IsAssignableFrom(val.[GetType]()) Then val = Nothing
            End If

            If val Is Nothing Then

                If prop.[Property].DefaultValue Is Nothing OrElse prop.[Property].DefaultValue.ToString() = "[null]" Then

                    If prop.[Property].PropertyType.IsValueType Then
                        Return Activator.CreateInstance(prop.[Property].PropertyType)
                    Else
                        Return Nothing
                    End If
                End If

                If Not (TypeOf prop.[Property].DefaultValue Is String) Then
                    val = prop.[Property].DefaultValue
                Else
                    val = GetObjectFromString(prop.[Property].PropertyType, prop.[Property].SerializeAs, CStr(prop.[Property].DefaultValue))
                End If

                If val IsNot Nothing AndAlso Not prop.[Property].PropertyType.IsAssignableFrom(val.[GetType]()) Then Throw New ArgumentException("Could not create from default value for property: " & prop.[Property].Name)
            End If

            If val Is Nothing Then

                If prop.[Property].PropertyType = GetType(String) Then
                    val = ""
                Else
                    val = Activator.CreateInstance(prop.[Property].PropertyType)
                End If
            End If

            Return val
        End Function

        Private Shared Sub PrepareDataForSaving(ByRef allNames As String, ByRef allValues As String, ByRef buf As Byte(), ByVal binarySupported As Boolean, ByVal properties As SettingsPropertyValueCollection, ByVal userIsAuthenticated As Boolean)
            Dim names As StringBuilder = New StringBuilder()
            Dim values As StringBuilder = New StringBuilder()
            Dim ms As MemoryStream = (If(binarySupported, New MemoryStream(), Nothing))

            Try
                Dim anyItemsToSave As Boolean = False

                For Each pp As SettingsPropertyValue In properties

                    If pp.IsDirty Then

                        If Not userIsAuthenticated Then
                            Dim allowAnonymous As Boolean = CBool(pp.[Property].Attributes("AllowAnonymous"))
                            If Not allowAnonymous Then Continue For
                        End If

                        anyItemsToSave = True
                        Exit For
                    End If
                Next

                If Not anyItemsToSave Then Return

                For Each pp As SettingsPropertyValue In properties

                    If Not userIsAuthenticated Then
                        Dim allowAnonymous As Boolean = CBool(pp.[Property].Attributes("AllowAnonymous"))
                        If Not allowAnonymous Then Continue For
                    End If

                    If Not pp.IsDirty AndAlso pp.UsingDefaultValue Then Continue For
                    Dim len As Integer, startPos As Integer = 0
                    Dim propValue As String = Nothing

                    If pp.Deserialized AndAlso pp.PropertyValue Is Nothing Then
                        len = -1
                    Else
                        Dim sVal As Object = SerializePropertyValue(pp)

                        If sVal Is Nothing Then
                            len = -1
                        Else

                            If Not (TypeOf sVal Is String) AndAlso Not binarySupported Then
                                sVal = Convert.ToBase64String(CType(sVal, Byte()))
                            End If

                            If TypeOf sVal Is String Then
                                propValue = CStr(sVal)
                                len = propValue.Length
                                startPos = values.Length
                            Else
                                Dim b2 As Byte() = CType(sVal, Byte())

                                If ms IsNot Nothing Then
                                    startPos = CInt(ms.Position)
                                    ms.Write(b2, 0, b2.Length)
                                    ms.Position = startPos + b2.Length
                                End If

                                len = b2.Length
                            End If
                        End If
                    End If

                    names.Append(pp.Name & ":" & (If((propValue IsNot Nothing), "S", "B")) & ":" & startPos.ToString(CultureInfo.InvariantCulture) & ":" & len.ToString(CultureInfo.InvariantCulture) & ":")
                    If propValue IsNot Nothing Then values.Append(propValue)
                Next

                If binarySupported Then
                    buf = ms.ToArray()
                End If

            Finally
                If ms IsNot Nothing Then ms.Close()
            End Try

            allNames = names.ToString()
            allValues = values.ToString()
        End Sub

        Private Shared Function ConvertObjectToString(ByVal propValue As Object, ByVal type As Type, ByVal serializeAs As SettingsSerializeAs, ByVal throwOnError As Boolean) As String
            If serializeAs = SettingsSerializeAs.ProviderSpecific Then

                If type = GetType(String) OrElse type.IsPrimitive Then
                    serializeAs = SettingsSerializeAs.String
                Else
                    serializeAs = SettingsSerializeAs.Xml
                End If
            End If

            Try

                Select Case serializeAs
                    Case SettingsSerializeAs.String
                        Dim converter As TypeConverter = TypeDescriptor.GetConverter(type)
                        If converter IsNot Nothing AndAlso converter.CanConvertTo(GetType(String)) AndAlso converter.CanConvertFrom(GetType(String)) Then Return converter.ConvertToString(propValue)
                        Throw New ArgumentException("Unable to convert type " & type.ToString() & " to string", "type")
                    Case SettingsSerializeAs.Binary
                        Dim ms As MemoryStream = New MemoryStream()

                        Try
                            Dim bf As BinaryFormatter = New BinaryFormatter()
                            bf.Serialize(ms, propValue)
                            Dim buffer As Byte() = ms.ToArray()
                            Return Convert.ToBase64String(buffer)
                        Finally
                            ms.Close()
                        End Try

                    Case SettingsSerializeAs.Xml
                        Dim xs As XmlSerializer = New XmlSerializer(type)
                        Dim sw As StringWriter = New StringWriter(CultureInfo.InvariantCulture)
                        xs.Serialize(sw, propValue)
                        Return sw.ToString()
                End Select

            Catch __unusedException1__ As Exception
                If throwOnError Then Throw
            End Try

            Return Nothing
        End Function

        Private Shared Function SerializePropertyValue(ByVal prop As SettingsPropertyValue) As Object
            Dim val As Object = prop.PropertyValue
            If val Is Nothing Then Return Nothing
            If prop.[Property].SerializeAs <> SettingsSerializeAs.Binary Then Return ConvertObjectToString(val, prop.[Property].PropertyType, prop.[Property].SerializeAs, prop.[Property].ThrowOnErrorSerializing)
            Dim ms As MemoryStream = New MemoryStream()

            Try
                Dim bf As BinaryFormatter = New BinaryFormatter()
                bf.Serialize(ms, val)
                Return ms.ToArray()
            Finally
                ms.Close()
            End Try
        End Function

        Private Shared Function GetObjectFromString(ByVal type As Type, ByVal serializeAs As SettingsSerializeAs, ByVal attValue As String) As Object
            If type = GetType(String) AndAlso (String.IsNullOrEmpty(attValue) OrElse serializeAs = SettingsSerializeAs.String) Then Return attValue
            If String.IsNullOrEmpty(attValue) Then Return Nothing

            Select Case serializeAs
                Case SettingsSerializeAs.Binary
                    Dim buf As Byte() = Convert.FromBase64String(attValue)
                    Dim ms As MemoryStream = Nothing

                    Try
                        ms = New MemoryStream(buf)
                        Return (New BinaryFormatter()).Deserialize(ms)
                    Finally
                        If ms IsNot Nothing Then ms.Close()
                    End Try

                Case SettingsSerializeAs.Xml
                    Dim sr As StringReader = New StringReader(attValue)
                    Dim xs As XmlSerializer = New XmlSerializer(type)
                    Return xs.Deserialize(sr)
                Case SettingsSerializeAs.String
                    Dim converter As TypeConverter = TypeDescriptor.GetConverter(type)
                    If converter IsNot Nothing AndAlso converter.CanConvertTo(GetType(String)) AndAlso converter.CanConvertFrom(GetType(String)) Then Return converter.ConvertFromString(attValue)
                    Throw New ArgumentException("Unable to convert type: " & type.ToString() & " from string", "type")
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Shared Function GetClauseForAuthenticationOptions(ByVal authenticationOption As ProfileAuthenticationOption) As String
            Select Case authenticationOption
                Case ProfileAuthenticationOption.Anonymous
                    Return " AND IsAnonymous='1' "
                Case ProfileAuthenticationOption.Authenticated
                    Return " AND IsAnonymous='0' "
                Case ProfileAuthenticationOption.All
                    Return " "
                Case Else
                    Throw New InvalidEnumArgumentException(String.Format("Unknown ProfileAuthenticationOption value: {0}.", authenticationOption.ToString()))
            End Select
        End Function

        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")>
        Private Shared Function GetDbConnectionForProfile() As SqliteConnection
            If System.Web.HttpContext.Current IsNot Nothing Then
                Dim tran As SqliteTransaction = CType(System.Web.HttpContext.Current.Items(HTTP_TRANSACTION_ID), SqliteTransaction)
                If (tran IsNot Nothing) AndAlso (String.Equals(tran.Connection.ConnectionString, _connectionString)) Then Return tran.Connection
            End If

            Return New SqliteConnection(_connectionString)
        End Function

        Private Shared Function IsTransactionInProgress() As Boolean
            If System.Web.HttpContext.Current Is Nothing Then Return False
            Dim tran As SqliteTransaction = CType(System.Web.HttpContext.Current.Items(HTTP_TRANSACTION_ID), SqliteTransaction)

            If (tran IsNot Nothing) AndAlso (String.Equals(tran.Connection.ConnectionString, _connectionString)) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
