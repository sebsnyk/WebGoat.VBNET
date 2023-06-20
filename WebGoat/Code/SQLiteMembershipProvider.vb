Imports System
Imports System.Collections.Specialized
Imports System.Configuration
Imports System.Configuration.Provider
Imports System.Data
Imports Mono.Data.Sqlite
Imports System.Globalization
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web.Security
Imports System.Runtime.InteropServices

Namespace TechInfoSystems.Data.SQLite
    Public NotInheritable Class SQLiteMembershipProvider
        Inherits MembershipProvider

        Private Const _httpTransactionId As String = "SQLiteTran"
        Private Const NEW_PASSWORD_LENGTH As Integer = 8
        Private Const APP_TB_NAME As String = "[aspnet_Applications]"
        Private Const USER_TB_NAME As String = "[aspnet_Users]"
        Private Const USERS_IN_ROLES_TB_NAME As String = "[aspnet_UsersInRoles]"
        Private Const PROFILE_TB_NAME As String = "[aspnet_Profile]"
        Private Const MAX_APPLICATION_NAME_LENGTH As Integer = 256
        Private Const MAX_USERNAME_LENGTH As Integer = 256
        Private Const MAX_PASSWORD_LENGTH As Integer = 128
        Private Const MAX_PASSWORD_ANSWER_LENGTH As Integer = 128
        Private Const MAX_EMAIL_LENGTH As Integer = 256
        Private Const MAX_PASSWORD_QUESTION_LENGTH As Integer = 256
        Private Shared _connectionString As String
        Private Shared _applicationName As String
        Private Shared _applicationId As String
        Private Shared _enablePasswordReset As Boolean
        Private Shared _enablePasswordRetrieval As Boolean
        Private Shared _requiresQuestionAndAnswer As Boolean
        Private Shared _requiresUniqueEmail As Boolean
        Private Shared _maxInvalidPasswordAttempts As Integer
        Private Shared _passwordAttemptWindow As Integer
        Private Shared _passwordFormat As MembershipPasswordFormat
        Private Shared _minRequiredNonAlphanumericCharacters As Integer
        Private Shared _minRequiredPasswordLength As Integer
        Private Shared _passwordStrengthRegularExpression As String
        Private Shared ReadOnly _minDate As DateTime = DateTime.ParseExact("01/01/1753", "d", CultureInfo.InvariantCulture)

        Public Overrides Property ApplicationName As String
            Get
                Return _applicationName
            End Get
            Set(ByVal value As String)
                If value.Length > MAX_APPLICATION_NAME_LENGTH Then Throw New ProviderException(String.Format("SQLiteMembershipProvider error: applicationName must be less than or equal to {0} characters.", MAX_APPLICATION_NAME_LENGTH))
                _applicationName = value
                _applicationId = GetApplicationId(_applicationName)
            End Set
        End Property

        Public Overrides ReadOnly Property EnablePasswordReset As Boolean
            Get
                Return _enablePasswordReset
            End Get
        End Property

        Public Overrides ReadOnly Property EnablePasswordRetrieval As Boolean
            Get
                Return _enablePasswordRetrieval
            End Get
        End Property

        Public Overrides ReadOnly Property RequiresQuestionAndAnswer As Boolean
            Get
                Return _requiresQuestionAndAnswer
            End Get
        End Property

        Public Overrides ReadOnly Property RequiresUniqueEmail As Boolean
            Get
                Return _requiresUniqueEmail
            End Get
        End Property

        Public Overrides ReadOnly Property MaxInvalidPasswordAttempts As Integer
            Get
                Return _maxInvalidPasswordAttempts
            End Get
        End Property

        Public Overrides ReadOnly Property PasswordAttemptWindow As Integer
            Get
                Return _passwordAttemptWindow
            End Get
        End Property

        Public Overrides ReadOnly Property PasswordFormat As MembershipPasswordFormat
            Get
                Return _passwordFormat
            End Get
        End Property

        Public Overrides ReadOnly Property MinRequiredNonAlphanumericCharacters As Integer
            Get
                Return _minRequiredNonAlphanumericCharacters
            End Get
        End Property

        Public Overrides ReadOnly Property MinRequiredPasswordLength As Integer
            Get
                Return _minRequiredPasswordLength
            End Get
        End Property

        Public Overrides ReadOnly Property PasswordStrengthRegularExpression As String
            Get
                Return _passwordStrengthRegularExpression
            End Get
        End Property

        Public Overrides Sub Initialize(ByVal name As String, ByVal config As NameValueCollection)
            If config Is Nothing Then Throw New ArgumentNullException("config")
            If String.IsNullOrEmpty(name) Then name = "SQLiteMembershipProvider"

            If String.IsNullOrEmpty(config("description")) Then
                config.Remove("description")
                config.Add("description", "SQLite Membership provider")
            End If

            MyBase.Initialize(name, config)
            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config("maxInvalidPasswordAttempts"), "50"))
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config("passwordAttemptWindow"), "10"))
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config("minRequiredNonalphanumericCharacters"), "1"))
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config("minRequiredPasswordLength"), "7"))
            _passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config("passwordStrengthRegularExpression"), ""))
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config("enablePasswordReset"), "true"))
            _enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config("enablePasswordRetrieval"), "false"))
            _requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config("requiresQuestionAndAnswer"), "false"))
            _requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config("requiresUniqueEmail"), "false"))
            ValidatePwdStrengthRegularExpression()

            If _minRequiredNonAlphanumericCharacters > _minRequiredPasswordLength Then
                Throw New System.Web.HttpException("SQLiteMembershipProvider configuration error: minRequiredNonalphanumericCharacters can not be greater than minRequiredPasswordLength. Check the web configuration file (web.config).")
            End If

            Dim temp_format As String = config("passwordFormat")

            If temp_format Is Nothing Then
                temp_format = "Hashed"
            End If

            Select Case temp_format
                Case "Clear"
                    _passwordFormat = MembershipPasswordFormat.Clear
                Case "Hashed"
                    _passwordFormat = MembershipPasswordFormat.Hashed
                Case "Encrypted"
                    _passwordFormat = MembershipPasswordFormat.Encrypted
                Case Else
                    Throw New ProviderException("Password format not supported.")
            End Select

            If (PasswordFormat = MembershipPasswordFormat.Hashed) AndAlso EnablePasswordRetrieval Then
                Throw New ProviderException("SQLiteMembershipProvider configuration error: enablePasswordRetrieval can not be set to true when passwordFormat is set to ""Hashed"". Check the web configuration file (web.config).")
            End If

            Dim ConnectionStringSettings As ConnectionStringSettings = ConfigurationManager.ConnectionStrings(config("connectionStringName"))

            If ConnectionStringSettings Is Nothing OrElse ConnectionStringSettings.ConnectionString Is Nothing OrElse ConnectionStringSettings.ConnectionString.Trim().Length = 0 Then
                Throw New ProviderException("Connection string is empty for SQLiteMembershipProvider. Check the web configuration file (web.config).")
            End If

            _connectionString = ConnectionStringSettings.ConnectionString

            If config("applicationName") Is Nothing OrElse config("applicationName").Trim() = "" Then
                Me.ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
            Else
                Me.ApplicationName = config("applicationName")
            End If

            config.Remove("connectionStringName")
            config.Remove("enablePasswordRetrieval")
            config.Remove("enablePasswordReset")
            config.Remove("requiresQuestionAndAnswer")
            config.Remove("applicationName")
            config.Remove("requiresUniqueEmail")
            config.Remove("maxInvalidPasswordAttempts")
            config.Remove("passwordAttemptWindow")
            config.Remove("commandTimeout")
            config.Remove("passwordFormat")
            config.Remove("name")
            config.Remove("minRequiredPasswordLength")
            config.Remove("minRequiredNonalphanumericCharacters")
            config.Remove("passwordStrengthRegularExpression")

            If config.Count > 0 Then
                Dim key As String = config.GetKey(0)

                If Not String.IsNullOrEmpty(key) Then
                    Throw New ProviderException(String.Concat("SQLiteMembershipProvider configuration error: Unrecognized attribute: ", key))
                End If
            End If

            VerifyApplication()
        End Sub

        Public Overrides Function ChangePassword(ByVal username As String, ByVal oldPassword As String, ByVal newPassword As String) As Boolean
            SecUtility.CheckParameter(username, True, True, True, MAX_USERNAME_LENGTH, "username")
            SecUtility.CheckParameter(oldPassword, True, True, False, MAX_PASSWORD_LENGTH, "oldPassword")
            SecUtility.CheckParameter(newPassword, True, True, False, MAX_PASSWORD_LENGTH, "newPassword")
            Dim salt As String
            Dim passwordFormat As MembershipPasswordFormat
            If Not CheckPassword(username, oldPassword, True, salt, passwordFormat) Then Return False

            If newPassword.Length < Me.MinRequiredPasswordLength Then
                Throw New ArgumentException(String.Format(CultureInfo.CurrentCulture, "The password must be at least {0} characters.", Me.MinRequiredPasswordLength))
            End If

            Dim numNonAlphaNumericChars As Integer = 0

            For i As Integer = 0 To newPassword.Length - 1

                If Not Char.IsLetterOrDigit(newPassword, i) Then
                    numNonAlphaNumericChars += 1
                End If
            Next

            If numNonAlphaNumericChars < Me.MinRequiredNonAlphanumericCharacters Then
                Throw New ArgumentException(String.Format(CultureInfo.CurrentCulture, "There must be at least {0} non alpha numeric characters.", Me.MinRequiredNonAlphanumericCharacters))
            End If

            If (Me.PasswordStrengthRegularExpression.Length > 0) AndAlso Not Regex.IsMatch(newPassword, Me.PasswordStrengthRegularExpression) Then
                Throw New ArgumentException("The password does not match the regular expression in the config file.")
            End If

            Dim encodedPwd As String = EncodePassword(newPassword, passwordFormat, salt)

            If encodedPwd.Length > MAX_PASSWORD_LENGTH Then
                Throw New ArgumentException(String.Format(CultureInfo.CurrentCulture, "The password is too long: it must not exceed {0} chars after encrypting.", MAX_PASSWORD_LENGTH))
            End If

            Dim args As ValidatePasswordEventArgs = New ValidatePasswordEventArgs(username, newPassword, False)
            OnValidatingPassword(args)

            If args.Cancel Then

                If args.FailureInformation IsNot Nothing Then
                    Throw args.FailureInformation
                Else
                    Throw New MembershipPasswordException("Change password canceled due to new password validation failure.")
                End If
            End If

            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET Password = $Password, LastPasswordChangedDate = $LastPasswordChangedDate " & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Password", encodedPwd)
                    cmd.Parameters.AddWithValue("$LastPasswordChangedDate", DateTime.UtcNow)
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Return (cmd.ExecuteNonQuery() > 0)
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function ChangePasswordQuestionAndAnswer(ByVal username As String, ByVal password As String, ByVal newPasswordQuestion As String, ByVal newPasswordAnswer As String) As Boolean
            SecUtility.CheckParameter(username, True, True, True, MAX_USERNAME_LENGTH, "username")
            SecUtility.CheckParameter(password, True, True, False, MAX_PASSWORD_LENGTH, "password")
            Dim salt, encodedPasswordAnswer As String
            Dim passwordFormat As MembershipPasswordFormat
            If Not CheckPassword(username, password, True, salt, passwordFormat) Then Return False
            SecUtility.CheckParameter(newPasswordQuestion, Me.RequiresQuestionAndAnswer, Me.RequiresQuestionAndAnswer, False, MAX_PASSWORD_QUESTION_LENGTH, "newPasswordQuestion")

            If newPasswordAnswer IsNot Nothing Then
                newPasswordAnswer = newPasswordAnswer.Trim()
            End If

            SecUtility.CheckParameter(newPasswordAnswer, Me.RequiresQuestionAndAnswer, Me.RequiresQuestionAndAnswer, False, MAX_PASSWORD_ANSWER_LENGTH, "newPasswordAnswer")

            If Not String.IsNullOrEmpty(newPasswordAnswer) Then
                encodedPasswordAnswer = EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt)
            Else
                encodedPasswordAnswer = newPasswordAnswer
            End If

            SecUtility.CheckParameter(encodedPasswordAnswer, Me.RequiresQuestionAndAnswer, Me.RequiresQuestionAndAnswer, False, MAX_PASSWORD_ANSWER_LENGTH, "newPasswordAnswer")
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET PasswordQuestion = $Question, PasswordAnswer = $Answer" & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Question", newPasswordQuestion)
                    cmd.Parameters.AddWithValue("$Answer", encodedPasswordAnswer)
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Return (cmd.ExecuteNonQuery() > 0)
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function CreateUser(ByVal username As String, ByVal password As String, ByVal email As String, ByVal passwordQuestion As String, ByVal passwordAnswer As String, ByVal isApproved As Boolean, ByVal providerUserKey As Object, <Out> ByRef status As MembershipCreateStatus) As MembershipUser
            If Not SecUtility.ValidateParameter(password, True, True, False, MAX_PASSWORD_LENGTH) Then
                status = MembershipCreateStatus.InvalidPassword
                Return Nothing
            End If

            Dim salt As String = GenerateSalt()
            Dim encodedPassword As String = EncodePassword(password, PasswordFormat, salt)

            If encodedPassword.Length > MAX_PASSWORD_LENGTH Then
                status = MembershipCreateStatus.InvalidPassword
                Return Nothing
            End If

            If passwordAnswer IsNot Nothing Then
                passwordAnswer = passwordAnswer.Trim()
            End If

            Dim encodedPasswordAnswer As String

            If Not String.IsNullOrEmpty(passwordAnswer) Then

                If passwordAnswer.Length > MAX_PASSWORD_ANSWER_LENGTH Then
                    status = MembershipCreateStatus.InvalidAnswer
                    Return Nothing
                End If

                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), PasswordFormat, salt)
            Else
                encodedPasswordAnswer = passwordAnswer
            End If

            If Not SecUtility.ValidateParameter(encodedPasswordAnswer, RequiresQuestionAndAnswer, True, False, MAX_PASSWORD_ANSWER_LENGTH) Then
                status = MembershipCreateStatus.InvalidAnswer
                Return Nothing
            End If

            If Not SecUtility.ValidateParameter(username, True, True, True, MAX_USERNAME_LENGTH) Then
                status = MembershipCreateStatus.InvalidUserName
                Return Nothing
            End If

            If Not SecUtility.ValidateParameter(email, Me.RequiresUniqueEmail, Me.RequiresUniqueEmail, False, MAX_EMAIL_LENGTH) Then
                status = MembershipCreateStatus.InvalidEmail
                Return Nothing
            End If

            If Not SecUtility.ValidateParameter(passwordQuestion, Me.RequiresQuestionAndAnswer, True, False, MAX_PASSWORD_QUESTION_LENGTH) Then
                status = MembershipCreateStatus.InvalidQuestion
                Return Nothing
            End If

            If (providerUserKey IsNot Nothing) AndAlso Not (TypeOf providerUserKey Is Guid) Then
                status = MembershipCreateStatus.InvalidProviderUserKey
                Return Nothing
            End If

            If password.Length < Me.MinRequiredPasswordLength Then
                status = MembershipCreateStatus.InvalidPassword
                Return Nothing
            End If

            Dim numNonAlphaNumericChars As Integer = 0

            For i As Integer = 0 To password.Length - 1

                If Not Char.IsLetterOrDigit(password, i) Then
                    numNonAlphaNumericChars += 1
                End If
            Next

            If numNonAlphaNumericChars < Me.MinRequiredNonAlphanumericCharacters Then
                status = MembershipCreateStatus.InvalidPassword
                Return Nothing
            End If

            If (Me.PasswordStrengthRegularExpression.Length > 0) AndAlso Not Regex.IsMatch(password, Me.PasswordStrengthRegularExpression) Then
                status = MembershipCreateStatus.InvalidPassword
                Return Nothing
            End If

            Dim args As ValidatePasswordEventArgs = New ValidatePasswordEventArgs(username, password, True)
            OnValidatingPassword(args)

            If args.Cancel Then
                status = MembershipCreateStatus.InvalidPassword
                Return Nothing
            End If

            If RequiresUniqueEmail AndAlso Not String.IsNullOrEmpty(GetUserNameByEmail(email)) Then
                status = MembershipCreateStatus.DuplicateEmail
                Return Nothing
            End If

            Dim u As MembershipUser = GetUser(username, False)

            If u Is Nothing Then
                Dim createDate As DateTime = DateTime.UtcNow

                If providerUserKey Is Nothing Then
                    providerUserKey = Guid.NewGuid()
                Else

                    If Not (TypeOf providerUserKey Is Guid) Then
                        status = MembershipCreateStatus.InvalidProviderUserKey
                        Return Nothing
                    End If
                End If

                Dim cn As SqliteConnection = GetDBConnectionForMembership()

                Try

                    Using cmd As SqliteCommand = cn.CreateCommand()
                        cmd.CommandText = "INSERT INTO " & USER_TB_NAME & " (UserId, Username, LoweredUsername, ApplicationId, Email, LoweredEmail, Comment, Password, " & " PasswordFormat, PasswordSalt, PasswordQuestion, PasswordAnswer, IsApproved, IsAnonymous, " & " LastActivityDate, LastLoginDate, LastPasswordChangedDate, CreateDate, " & " IsLockedOut, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart, " & " FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart) " & " Values ($UserId, $Username, $LoweredUsername, $ApplicationId, $Email, $LoweredEmail, $Comment, $Password, " & " $PasswordFormat, $PasswordSalt, $PasswordQuestion, $PasswordAnswer, $IsApproved, $IsAnonymous, " & " $LastActivityDate, $LastLoginDate, $LastPasswordChangedDate, $CreateDate, " & " $IsLockedOut, $LastLockoutDate, $FailedPasswordAttemptCount, $FailedPasswordAttemptWindowStart, " & " $FailedPasswordAnswerAttemptCount, $FailedPasswordAnswerAttemptWindowStart)"
                        Dim nullDate As DateTime = _minDate
                        cmd.Parameters.AddWithValue("$UserId", providerUserKey.ToString())
                        cmd.Parameters.AddWithValue("$Username", username)
                        cmd.Parameters.AddWithValue("$LoweredUsername", username.ToLowerInvariant())
                        cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                        cmd.Parameters.AddWithValue("$Email", email)
                        cmd.Parameters.AddWithValue("$LoweredEmail", (If(email IsNot Nothing, email.ToLowerInvariant(), Nothing)))
                        cmd.Parameters.AddWithValue("$Comment", Nothing)
                        cmd.Parameters.AddWithValue("$Password", encodedPassword)
                        cmd.Parameters.AddWithValue("$PasswordFormat", PasswordFormat.ToString())
                        cmd.Parameters.AddWithValue("$PasswordSalt", salt)
                        cmd.Parameters.AddWithValue("$PasswordQuestion", passwordQuestion)
                        cmd.Parameters.AddWithValue("$PasswordAnswer", encodedPasswordAnswer)
                        cmd.Parameters.AddWithValue("$IsApproved", isApproved)
                        cmd.Parameters.AddWithValue("$IsAnonymous", False)
                        cmd.Parameters.AddWithValue("$LastActivityDate", createDate)
                        cmd.Parameters.AddWithValue("$LastLoginDate", createDate)
                        cmd.Parameters.AddWithValue("$LastPasswordChangedDate", createDate)
                        cmd.Parameters.AddWithValue("$CreateDate", createDate)
                        cmd.Parameters.AddWithValue("$IsLockedOut", False)
                        cmd.Parameters.AddWithValue("$LastLockoutDate", nullDate)
                        cmd.Parameters.AddWithValue("$FailedPasswordAttemptCount", 0)
                        cmd.Parameters.AddWithValue("$FailedPasswordAttemptWindowStart", nullDate)
                        cmd.Parameters.AddWithValue("$FailedPasswordAnswerAttemptCount", 0)
                        cmd.Parameters.AddWithValue("$FailedPasswordAnswerAttemptWindowStart", nullDate)
                        If cn.State = ConnectionState.Closed Then cn.Open()

                        If cmd.ExecuteNonQuery() > 0 Then
                            status = MembershipCreateStatus.Success
                        Else
                            status = MembershipCreateStatus.UserRejected
                        End If
                    End Using

                Catch
                    status = MembershipCreateStatus.ProviderError
                    Throw
                Finally
                    If Not IsTransactionInProgress() Then cn.Dispose()
                End Try

                Return GetUser(username, False)
            Else
                status = MembershipCreateStatus.DuplicateUserName
            End If

            Return Nothing
        End Function

        Public Overrides Function DeleteUser(ByVal username As String, ByVal deleteAllRelatedData As Boolean) As Boolean
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Dim userId As String = Nothing

                    If deleteAllRelatedData Then
                        cmd.CommandText = "SELECT UserId FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                        cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                        cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                        userId = TryCast(cmd.ExecuteScalar(), String)
                    End If

                    cmd.CommandText = "DELETE FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If deleteAllRelatedData AndAlso (Not String.IsNullOrEmpty((userId))) Then
                        cmd.CommandText = "DELETE FROM " & USERS_IN_ROLES_TB_NAME & " WHERE UserId = $UserId"
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("$UserId", userId)
                        cmd.ExecuteNonQuery()
                        cmd.CommandText = "DELETE FROM " & PROFILE_TB_NAME & " WHERE UserId = $UserId"
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("$UserId", userId)
                        cmd.ExecuteNonQuery()
                    End If

                    Return (rowsAffected > 0)
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetAllUsers(ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As MembershipUserCollection
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT Count(*) FROM " & USER_TB_NAME & " WHERE ApplicationId = $ApplicationId AND IsAnonymous='0'"
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    totalRecords = Convert.ToInt32(cmd.ExecuteScalar())
                    Dim users As MembershipUserCollection = New MembershipUserCollection()

                    If totalRecords <= 0 Then
                        Return users
                    End If

                    cmd.CommandText = "SELECT UserId, Username, Email, PasswordQuestion," & " Comment, IsApproved, IsLockedOut, CreateDate, LastLoginDate," & " LastActivityDate, LastPasswordChangedDate, LastLockoutDate " & " FROM " & USER_TB_NAME & " WHERE ApplicationId = $ApplicationId AND IsAnonymous='0' " & " ORDER BY Username Asc"

                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        Dim counter As Integer = 0
                        Dim startIndex As Integer = pageSize * pageIndex
                        Dim endIndex As Integer = startIndex + pageSize - 1

                        While reader.Read()

                            If counter >= startIndex Then
                                Dim u As MembershipUser = GetUserFromReader(reader)
                                users.Add(u)
                            End If

                            If counter >= endIndex Then
                                cmd.Cancel()
                            End If

                            counter += 1
                        End While

                        Return users
                    End Using
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetNumberOfUsersOnline() As Integer
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT Count(*) FROM " & USER_TB_NAME & " WHERE LastActivityDate > $LastActivityDate AND ApplicationId = $ApplicationId"
                    Dim onlineSpan As TimeSpan = New TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0)
                    Dim compareTime As DateTime = DateTime.UtcNow.Subtract(onlineSpan)
                    cmd.Parameters.AddWithValue("$LastActivityDate", compareTime)
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetPassword(ByVal username As String, ByVal answer As String) As String
            If Not EnablePasswordRetrieval Then
                Throw New ProviderException("Password retrieval not enabled.")
            End If

            If PasswordFormat = MembershipPasswordFormat.Hashed Then
                Throw New ProviderException("Cannot retrieve hashed passwords.")
            End If

            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT Password, PasswordFormat, PasswordSalt, PasswordAnswer, IsLockedOut FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()

                    Using dr As SqliteDataReader = cmd.ExecuteReader((CommandBehavior.SingleRow))
                        Dim password, passwordAnswer, passwordSalt As String
                        Dim passwordFormat As MembershipPasswordFormat

                        If dr.HasRows Then
                            dr.Read()
                            If dr.GetBoolean(4) Then Throw New MembershipPasswordException("The supplied user is locked out.")
                            password = dr.GetString(0)
                            passwordFormat = CType([Enum].Parse(GetType(MembershipPasswordFormat), dr.GetString(1)), MembershipPasswordFormat)
                            passwordSalt = dr.GetString(2)
                            passwordAnswer = (If(dr.GetValue(3) = DBNull.Value, String.Empty, dr.GetString(3)))
                        Else
                            Throw New MembershipPasswordException("The supplied user name is not found.")
                        End If

                        If RequiresQuestionAndAnswer AndAlso Not ComparePasswords(answer, passwordAnswer, passwordSalt, passwordFormat) Then
                            UpdateFailureCount(username, "passwordAnswer", False)
                            Throw New MembershipPasswordException("Incorrect password answer.")
                        End If

                        If passwordFormat = MembershipPasswordFormat.Encrypted Then
                            password = UnEncodePassword(password, passwordFormat)
                        End If

                        Return password
                    End Using
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetUser(ByVal username As String, ByVal userIsOnline As Boolean) As MembershipUser
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT UserId, Username, Email, PasswordQuestion," & " Comment, IsApproved, IsLockedOut, CreateDate, LastLoginDate," & " LastActivityDate, LastPasswordChangedDate, LastLockoutDate" & " FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    Dim user As MembershipUser = Nothing
                    If cn.State = ConnectionState.Closed Then cn.Open()

                    Using dr As SqliteDataReader = cmd.ExecuteReader()

                        If dr.HasRows Then
                            dr.Read()
                            user = GetUserFromReader(dr)
                        End If
                    End Using

                    If userIsOnline Then
                        cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET LastActivityDate = $LastActivityDate" & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                        cmd.Parameters.AddWithValue("$LastActivityDate", DateTime.UtcNow)
                        cmd.ExecuteNonQuery()
                    End If

                    Return user
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetUser(ByVal providerUserKey As Object, ByVal userIsOnline As Boolean) As MembershipUser
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT UserId, Username, Email, PasswordQuestion," & " Comment, IsApproved, IsLockedOut, CreateDate, LastLoginDate," & " LastActivityDate, LastPasswordChangedDate, LastLockoutDate" & " FROM " & USER_TB_NAME & " WHERE UserId = $UserId"
                    cmd.Parameters.AddWithValue("$UserId", providerUserKey.ToString())
                    Dim user As MembershipUser = Nothing
                    If cn.State = ConnectionState.Closed Then cn.Open()

                    Using dr As SqliteDataReader = cmd.ExecuteReader()

                        If dr.HasRows Then
                            dr.Read()
                            user = GetUserFromReader(dr)
                        End If
                    End Using

                    If userIsOnline Then
                        cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET LastActivityDate = $LastActivityDate" & " WHERE UserId = $UserId"
                        cmd.Parameters.AddWithValue("$LastActivityDate", DateTime.UtcNow)
                        cmd.ExecuteNonQuery()
                    End If

                    Return user
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function UnlockUser(ByVal username As String) As Boolean
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET IsLockedOut = '0', FailedPasswordAttemptCount = 0," & " FailedPasswordAttemptWindowStart = $MinDate, FailedPasswordAnswerAttemptCount = 0," & " FailedPasswordAnswerAttemptWindowStart = $MinDate" & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$MinDate", _minDate)
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Return (cmd.ExecuteNonQuery() > 0)
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function GetUserNameByEmail(ByVal email As String) As String
            If email Is Nothing Then Return Nothing
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT Username" & " FROM " & USER_TB_NAME & " WHERE LoweredEmail = $Email AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Email", email.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    Return (TryCast(cmd.ExecuteScalar(), String))
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function ResetPassword(ByVal username As String, ByVal passwordAnswer As String) As String
            Dim salt As String
            Dim passwordFormat As MembershipPasswordFormat
            Dim passwordFromDb As String
            Dim status As Integer
            Dim failedPwdAttemptCount As Integer
            Dim failedPwdAnswerAttemptCount As Integer
            Dim isApproved As Boolean
            Dim lastLoginDate As DateTime
            Dim lastActivityDate As DateTime

            If Not Me.EnablePasswordReset Then
                Throw New NotSupportedException("This provider is not configured to allow password resets. To enable password reset, set enablePasswordReset to ""true"" in the configuration file.")
            End If

            SecUtility.CheckParameter(username, True, True, True, &H100, "username")
            GetPasswordWithFormat(username, status, passwordFromDb, passwordFormat, salt, failedPwdAttemptCount, failedPwdAnswerAttemptCount, isApproved, lastLoginDate, lastActivityDate)

            If status <> 0 Then

                If IsStatusDueToBadPassword(status) Then
                    Throw New MembershipPasswordException(GetExceptionText(status))
                End If

                Throw New ProviderException(GetExceptionText(status))
            End If

            Dim encodedPwdAnswer As String

            If passwordAnswer IsNot Nothing Then
                passwordAnswer = passwordAnswer.Trim()
            End If

            If Not String.IsNullOrEmpty(passwordAnswer) Then
                encodedPwdAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt)
            Else
                encodedPwdAnswer = passwordAnswer
            End If

            SecUtility.CheckParameter(encodedPwdAnswer, Me.RequiresQuestionAndAnswer, Me.RequiresQuestionAndAnswer, False, MAX_PASSWORD_ANSWER_LENGTH, "passwordAnswer")
            Dim newPassword As String = Membership.GeneratePassword(NEW_PASSWORD_LENGTH, MinRequiredNonAlphanumericCharacters)
            Dim e As ValidatePasswordEventArgs = New ValidatePasswordEventArgs(username, newPassword, False)
            Me.OnValidatingPassword(e)

            If e.Cancel Then

                If e.FailureInformation IsNot Nothing Then
                    Throw e.FailureInformation
                End If

                Throw New ProviderException("The custom password validation failed.")
            End If

            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT PasswordAnswer, IsLockedOut FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()

                    Using dr As SqliteDataReader = cmd.ExecuteReader(CommandBehavior.SingleRow)
                        Dim passwordAnswerFromDb As String

                        If dr.HasRows Then
                            dr.Read()
                            If Convert.ToBoolean(dr.GetValue(1)) Then Throw New MembershipPasswordException("The supplied user is locked out.")
                            passwordAnswerFromDb = TryCast(dr.GetValue(0), String)
                        Else
                            Throw New MembershipPasswordException("The supplied user name is not found.")
                        End If

                        If RequiresQuestionAndAnswer AndAlso Not ComparePasswords(passwordAnswer, passwordAnswerFromDb, salt, passwordFormat) Then
                            UpdateFailureCount(username, "passwordAnswer", False)
                            Throw New MembershipPasswordException("Incorrect password answer.")
                        End If
                    End Using

                    cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET Password = $Password, LastPasswordChangedDate = $LastPasswordChangedDate," & " FailedPasswordAttemptCount = 0, FailedPasswordAttemptWindowStart = $MinDate," & " FailedPasswordAnswerAttemptCount = 0, FailedPasswordAnswerAttemptWindowStart = $MinDate" & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId AND IsLockedOut = 0"
                    cmd.Parameters.Clear()
                    cmd.Parameters.AddWithValue("$Password", EncodePassword(newPassword, passwordFormat, salt))
                    cmd.Parameters.AddWithValue("$LastPasswordChangedDate", DateTime.UtcNow)
                    cmd.Parameters.AddWithValue("$MinDate", _minDate)
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)

                    If cmd.ExecuteNonQuery() > 0 Then
                        Return newPassword
                    Else
                        Throw New MembershipPasswordException("User not found, or user is locked out. Password not reset.")
                    End If
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Sub UpdateUser(ByVal user As MembershipUser)
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET Email = $Email, LoweredEmail = $LoweredEmail, Comment = $Comment," & " IsApproved = $IsApproved" & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Email", user.Email)
                    cmd.Parameters.AddWithValue("$LoweredEmail", user.Email.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$Comment", user.Comment)
                    cmd.Parameters.AddWithValue("$IsApproved", user.IsApproved)
                    cmd.Parameters.AddWithValue("$Username", user.UserName.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    cmd.ExecuteNonQuery()
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Sub

        Public Overrides Function ValidateUser(ByVal username As String, ByVal password As String) As Boolean
            If Not SecUtility.ValidateParameter(username, True, True, True, MAX_USERNAME_LENGTH) OrElse Not SecUtility.ValidateParameter(password, True, True, False, MAX_PASSWORD_LENGTH) Then
                Return False
            End If

            Dim salt As String
            Dim passwordFormat As MembershipPasswordFormat
            Dim isAuthenticated As Boolean = CheckPassword(username, password, True, salt, passwordFormat)

            If isAuthenticated Then
                Dim cn As SqliteConnection = GetDBConnectionForMembership()

                Try

                    Using cmd As SqliteCommand = cn.CreateCommand()
                        cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET LastActivityDate = $UtcNow, LastLoginDate = $UtcNow" & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                        cmd.Parameters.AddWithValue("$UtcNow", DateTime.UtcNow)
                        cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                        cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                        If cn.State = ConnectionState.Closed Then cn.Open()
                        cmd.ExecuteNonQuery()
                    End Using

                Finally
                    If Not IsTransactionInProgress() Then cn.Dispose()
                End Try
            End If

            Return isAuthenticated
        End Function

        Public Overrides Function FindUsersByName(ByVal usernameToMatch As String, ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As MembershipUserCollection
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT Count(*) FROM " & USER_TB_NAME & "WHERE LoweredUsername LIKE $UsernameSearch AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$UsernameSearch", usernameToMatch.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    totalRecords = Convert.ToInt32(cmd.ExecuteScalar())
                    Dim users As MembershipUserCollection = New MembershipUserCollection()

                    If totalRecords <= 0 Then
                        Return users
                    End If

                    cmd.CommandText = "SELECT UserId, Username, Email, PasswordQuestion," & " Comment, IsApproved, IsLockedOut, CreateDate, LastLoginDate," & " LastActivityDate, LastPasswordChangedDate, LastLockoutDate " & " FROM " & USER_TB_NAME & " WHERE LoweredUsername LIKE $UsernameSearch AND ApplicationId = $ApplicationId " & " ORDER BY Username Asc"

                    Using dr As SqliteDataReader = cmd.ExecuteReader()
                        Dim counter As Integer = 0
                        Dim startIndex As Integer = pageSize * pageIndex
                        Dim endIndex As Integer = startIndex + pageSize - 1

                        While dr.Read()

                            If counter >= startIndex Then
                                Dim u As MembershipUser = GetUserFromReader(dr)
                                users.Add(u)
                            End If

                            If counter >= endIndex Then
                                cmd.Cancel()
                            End If

                            counter += 1
                        End While
                    End Using

                    Return users
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Public Overrides Function FindUsersByEmail(ByVal emailToMatch As String, ByVal pageIndex As Integer, ByVal pageSize As Integer, <Out> ByRef totalRecords As Integer) As MembershipUserCollection
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT Count(*) FROM " & USER_TB_NAME & " WHERE LoweredEmail LIKE $EmailSearch AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$EmailSearch", emailToMatch.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    totalRecords = Convert.ToInt32(cmd.ExecuteScalar())
                    Dim users As MembershipUserCollection = New MembershipUserCollection()

                    If totalRecords <= 0 Then
                        Return users
                    End If

                    cmd.CommandText = "SELECT UserId, Username, Email, PasswordQuestion," & " Comment, IsApproved, IsLockedOut, CreateDate, LastLoginDate," & " LastActivityDate, LastPasswordChangedDate, LastLockoutDate" & " FROM " & USER_TB_NAME & " WHERE LoweredEmail LIKE $EmailSearch AND ApplicationId = $ApplicationId" & " ORDER BY Username Asc"

                    Using dr As SqliteDataReader = cmd.ExecuteReader()
                        Dim counter As Integer = 0
                        Dim startIndex As Integer = pageSize * pageIndex
                        Dim endIndex As Integer = startIndex + pageSize - 1

                        While dr.Read()

                            If counter >= startIndex Then
                                Dim u As MembershipUser = GetUserFromReader(dr)
                                users.Add(u)
                            End If

                            If counter >= endIndex Then
                                cmd.Cancel()
                            End If

                            counter += 1
                        End While
                    End Using

                    Return users
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Function

        Private Shared Sub ValidatePwdStrengthRegularExpression()
            If _passwordStrengthRegularExpression Is Nothing Then _passwordStrengthRegularExpression = String.Empty
            _passwordStrengthRegularExpression = _passwordStrengthRegularExpression.Trim()

            If _passwordStrengthRegularExpression.Length > 0 Then

                Try
                    New Regex(_passwordStrengthRegularExpression)
                Catch ex As ArgumentException
                    Throw New ProviderException(ex.Message, ex)
                End Try
            End If
        End Sub

        Private Shared Sub VerifyApplication()
            If Not String.IsNullOrEmpty(_applicationId) Then Return
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "INSERT INTO " & APP_TB_NAME & " (ApplicationId, ApplicationName, Description) VALUES ($ApplicationId, $ApplicationName, $Description)"
                    _applicationId = Guid.NewGuid().ToString()
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    cmd.Parameters.AddWithValue("ApplicationName", _applicationName)
                    cmd.Parameters.AddWithValue("Description", String.Empty)
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    cmd.ExecuteNonQuery()
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Sub

        Private Shared Function GetApplicationId(ByVal appName As String) As String
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

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

        Private Shared Function GetConfigValue(ByVal configValue As String, ByVal defaultValue As String) As String
            If String.IsNullOrEmpty(configValue) Then Return defaultValue
            Return configValue
        End Function

        Private Function GetUserFromReader(ByVal reader As IDataRecord) As MembershipUser
            If reader.GetString(1) = "" Then Return Nothing
            Dim providerUserKey As Object = Nothing
            Dim strGooid As String = Guid.NewGuid().ToString()

            If reader.GetValue(0).ToString().Length > 0 Then
                providerUserKey = New Guid(reader.GetValue(0).ToString())
            Else
                providerUserKey = New Guid(strGooid)
            End If

            Dim username As String = reader.GetString(1)
            Dim email As String = (If(reader.GetValue(2) = DBNull.Value, String.Empty, reader.GetString(2)))
            Dim passwordQuestion As String = (If(reader.GetValue(3) = DBNull.Value, String.Empty, reader.GetString(3)))
            Dim comment As String = (If(reader.GetValue(4) = DBNull.Value, String.Empty, reader.GetString(4)))
            Dim isApproved As Boolean = reader.GetBoolean(5)
            Dim isLockedOut As Boolean = reader.GetBoolean(6)
            Dim creationDate As DateTime = reader.GetDateTime(7)
            Dim lastLoginDate As DateTime = reader.GetDateTime(8)
            Dim lastActivityDate As DateTime = reader.GetDateTime(9)
            Dim lastPasswordChangedDate As DateTime = reader.GetDateTime(10)
            Dim lastLockedOutDate As DateTime = reader.GetDateTime(11)
            Dim user As MembershipUser = New MembershipUser(Me.Name, username, providerUserKey, email, passwordQuestion, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockedOutDate)
            Return user
        End Function

        Private Sub UpdateFailureCount(ByVal username As String, ByVal failureType As String, ByVal isAuthenticated As Boolean)
            If Not ((failureType = "password") OrElse (failureType = "passwordAnswer")) Then
                Throw New ArgumentException("Invalid value for failureType parameter. Must be 'password' or 'passwordAnswer'.", "failureType")
            End If

            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart, " & "  FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart, IsLockedOut " & "  FROM " & USER_TB_NAME & "  WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    Dim failedPasswordAttemptCount As Integer = 0
                    Dim failedPasswordAnswerAttemptCount As Integer = 0
                    Dim failedPasswordAttemptWindowStart As DateTime = _minDate
                    Dim failedPasswordAnswerAttemptWindowStart As DateTime = _minDate
                    Dim isLockedOut As Boolean = False
                    If cn.State = ConnectionState.Closed Then cn.Open()

                    Using dr As SqliteDataReader = cmd.ExecuteReader(CommandBehavior.SingleRow)

                        If dr.HasRows Then
                            dr.Read()
                            failedPasswordAttemptCount = dr.GetInt32(0)
                            failedPasswordAttemptWindowStart = dr.GetDateTime(1)
                            failedPasswordAnswerAttemptCount = dr.GetInt32(2)
                            failedPasswordAnswerAttemptWindowStart = dr.GetDateTime(3)
                            isLockedOut = dr.GetBoolean(4)
                        End If
                    End Using

                    If isLockedOut Then Return

                    If isAuthenticated Then

                        If (failedPasswordAttemptCount > 0) OrElse (failedPasswordAnswerAttemptCount > 0) Then
                            cmd.CommandText = "UPDATE " & USER_TB_NAME & " SET FailedPasswordAttemptCount = 0, FailedPasswordAttemptWindowStart = $MinDate, " & " FailedPasswordAnswerAttemptCount = 0, FailedPasswordAnswerAttemptWindowStart = $MinDate, IsLockedOut = '0' " & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                            cmd.Parameters.Clear()
                            cmd.Parameters.AddWithValue("$MinDate", _minDate)
                            cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                            cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                            cmd.ExecuteNonQuery()
                        End If

                        Return
                    End If

                    Dim windowStart As DateTime = _minDate
                    Dim failureCount As Integer = 0

                    If failureType = "password" Then
                        windowStart = failedPasswordAttemptWindowStart
                        failureCount = failedPasswordAttemptCount
                    ElseIf failureType = "passwordAnswer" Then
                        windowStart = failedPasswordAnswerAttemptWindowStart
                        failureCount = failedPasswordAnswerAttemptCount
                    End If

                    Dim windowEnd As DateTime = windowStart.AddMinutes(PasswordAttemptWindow)

                    If failureCount = 0 OrElse DateTime.UtcNow > windowEnd Then
                        If failureType = "password" Then cmd.CommandText = "UPDATE " & USER_TB_NAME & "  SET FailedPasswordAttemptCount = $Count, " & "      FailedPasswordAttemptWindowStart = $WindowStart " & "  WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                        If failureType = "passwordAnswer" Then cmd.CommandText = "UPDATE " & USER_TB_NAME & "  SET FailedPasswordAnswerAttemptCount = $Count, " & "      FailedPasswordAnswerAttemptWindowStart = $WindowStart " & "  WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("$Count", 1)
                        cmd.Parameters.AddWithValue("$WindowStart", DateTime.UtcNow)
                        cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                        cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                        If cmd.ExecuteNonQuery() < 0 Then Throw New ProviderException("Unable to update failure count and window start.")
                    Else

                        If Math.Min(System.Threading.Interlocked.Increment(failureCount), failureCount - 1) >= MaxInvalidPasswordAttempts Then
                            cmd.CommandText = "UPDATE " & USER_TB_NAME & "  SET IsLockedOut = '1', LastLockoutDate = $LastLockoutDate, FailedPasswordAttemptCount = $Count " & "  WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                            cmd.Parameters.Clear()
                            cmd.Parameters.AddWithValue("$LastLockoutDate", DateTime.UtcNow)
                            cmd.Parameters.AddWithValue("$Count", failureCount)
                            cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                            cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                            If cmd.ExecuteNonQuery() < 0 Then Throw New ProviderException("Unable to lock out user.")
                        Else
                            If failureType = "password" Then cmd.CommandText = "UPDATE " & USER_TB_NAME & "  SET FailedPasswordAttemptCount = $Count" & "  WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                            If failureType = "passwordAnswer" Then cmd.CommandText = "UPDATE " & USER_TB_NAME & "  SET FailedPasswordAnswerAttemptCount = $Count" & "  WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                            cmd.Parameters.Clear()
                            cmd.Parameters.AddWithValue("$Count", failureCount)
                            cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                            cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                            If cmd.ExecuteNonQuery() < 0 Then Throw New ProviderException("Unable to update failure count.")
                        End If
                    End If
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Sub

        Private Function ComparePasswords(ByVal password As String, ByVal dbpassword As String, ByVal salt As String, ByVal passwordFormat As MembershipPasswordFormat) As Boolean
            Dim pass1 As String = password
            Dim pass2 As String = dbpassword

            Select Case passwordFormat
                Case MembershipPasswordFormat.Encrypted
                    pass2 = UnEncodePassword(dbpassword, passwordFormat)
                Case MembershipPasswordFormat.Hashed
                    pass1 = EncodePassword(password, passwordFormat, salt)
                Case Else
            End Select

            If pass1 = pass2 Then
                Return True
            End If

            Return False
        End Function

        Private Function CheckPassword(ByVal username As String, ByVal password As String, ByVal failIfNotApproved As Boolean, <Out> ByRef salt As String, <Out> ByRef passwordFormat As MembershipPasswordFormat) As Boolean
            Dim encodedPwdFromDatabase As String
            Dim status As Integer
            Dim failedPwdAttemptCount As Integer
            Dim failedPwdAnswerAttemptCount As Integer
            Dim isApproved As Boolean
            Dim lastLoginDate As DateTime
            Dim lastActivityDate As DateTime
            GetPasswordWithFormat(username, status, encodedPwdFromDatabase, passwordFormat, salt, failedPwdAttemptCount, failedPwdAnswerAttemptCount, isApproved, lastLoginDate, lastActivityDate)

            If status <> 0 Then
                Return False
            End If

            If Not isApproved AndAlso failIfNotApproved Then
                Return False
            End If

            Dim encodedPwdFromUser As String = EncodePassword(password, passwordFormat, salt)
            Dim isAuthenticated As Boolean = encodedPwdFromDatabase.Equals(encodedPwdFromUser)

            If (isAuthenticated AndAlso (failedPwdAttemptCount = 0)) AndAlso (failedPwdAnswerAttemptCount = 0) Then
                Return True
            End If

            UpdateFailureCount(username, "password", isAuthenticated)
            Return isAuthenticated
        End Function

        Private Shared Sub GetPasswordWithFormat(ByVal username As String, <Out> ByRef status As Integer, <Out> ByRef password As String, <Out> ByRef passwordFormat As MembershipPasswordFormat, <Out> ByRef passwordSalt As String, <Out> ByRef failedPasswordAttemptCount As Integer, <Out> ByRef failedPasswordAnswerAttemptCount As Integer, <Out> ByRef isApproved As Boolean, <Out> ByRef lastLoginDate As DateTime, <Out> ByRef lastActivityDate As DateTime)
            Dim cn As SqliteConnection = GetDBConnectionForMembership()

            Try

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "SELECT Password, PasswordFormat, PasswordSalt, FailedPasswordAttemptCount," & " FailedPasswordAnswerAttemptCount, IsApproved, IsLockedOut, LastLoginDate, LastActivityDate" & " FROM " & USER_TB_NAME & " WHERE LoweredUsername = $Username AND ApplicationId = $ApplicationId"
                    cmd.Parameters.AddWithValue("$Username", username.ToLowerInvariant())
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)
                    If cn.State = ConnectionState.Closed Then cn.Open()

                    Using dr As SqliteDataReader = cmd.ExecuteReader(CommandBehavior.SingleRow)

                        If dr.HasRows Then
                            dr.Read()
                            password = dr.GetString(0)
                            passwordFormat = CType([Enum].Parse(GetType(MembershipPasswordFormat), dr.GetString(1)), MembershipPasswordFormat)
                            passwordSalt = dr.GetString(2)
                            failedPasswordAttemptCount = dr.GetInt32(3)
                            failedPasswordAnswerAttemptCount = dr.GetInt32(4)
                            isApproved = dr.GetBoolean(5)
                            status = If(dr.GetBoolean(6), 99, 0)
                            lastLoginDate = dr.GetDateTime(7)
                            lastActivityDate = dr.GetDateTime(8)
                        Else
                            status = 1
                            password = Nothing
                            passwordFormat = MembershipPasswordFormat.Clear
                            passwordSalt = Nothing
                            failedPasswordAttemptCount = 0
                            failedPasswordAnswerAttemptCount = 0
                            isApproved = False
                            lastLoginDate = DateTime.UtcNow
                            lastActivityDate = DateTime.UtcNow
                        End If
                    End Using
                End Using

            Finally
                If Not IsTransactionInProgress() Then cn.Dispose()
            End Try
        End Sub

        Private Function EncodePassword(ByVal password As String, ByVal passwordFormat As MembershipPasswordFormat, ByVal salt As String) As String
            If String.IsNullOrEmpty(password) Then Return password
            Dim bytes As Byte() = Encoding.Unicode.GetBytes(password)
            Dim src As Byte() = Convert.FromBase64String(salt)
            Dim dst As Byte() = New Byte(src.Length + bytes.Length - 1) {}
            Dim inArray As Byte()
            Buffer.BlockCopy(src, 0, dst, 0, src.Length)
            Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length)

            Select Case passwordFormat
                Case MembershipPasswordFormat.Clear
                    Return password
                Case MembershipPasswordFormat.Encrypted
                    inArray = EncryptPassword(dst)
                Case MembershipPasswordFormat.Hashed
                    Dim algorithm As HashAlgorithm = HashAlgorithm.Create(Membership.HashAlgorithmType)

                    If algorithm Is Nothing Then
                        Throw New ProviderException(String.Concat("SQLiteMembershipProvider configuration error: HashAlgorithm.Create() does not recognize the hash algorithm ", Membership.HashAlgorithmType, "."))
                    End If

                    inArray = algorithm.ComputeHash(dst)
                Case Else
                    Throw New ProviderException("Unsupported password format.")
            End Select

            Return Convert.ToBase64String(inArray)
        End Function

        Private Function UnEncodePassword(ByVal encodedPassword As String, ByVal passwordFormat As MembershipPasswordFormat) As String
            Dim password As String = encodedPassword

            Select Case passwordFormat
                Case MembershipPasswordFormat.Clear
                Case MembershipPasswordFormat.Encrypted
                    Dim bytes As Byte() = MyBase.DecryptPassword(Convert.FromBase64String(password))

                    If bytes Is Nothing Then
                        password = Nothing
                    Else
                        password = Encoding.Unicode.GetString(bytes, &H10, bytes.Length - &H10)
                    End If

                Case MembershipPasswordFormat.Hashed
                    Throw New ProviderException("Cannot unencode a hashed password.")
                Case Else
                    Throw New ProviderException("Unsupported password format.")
            End Select

            Return password
        End Function

        Private Shared Function HexToByte(ByVal hexString As String) As Byte()
            Dim returnBytes As Byte() = New Byte(hexString.Length / 2 - 1) {}

            For i As Integer = 0 To returnBytes.Length - 1
                returnBytes(i) = Convert.ToByte(hexString.Substring(i * 2, 2), 16)
            Next

            Return returnBytes
        End Function

        Private Shared Function GenerateSalt() As String
            Dim data As Byte() = New Byte(15) {}
            New RNGCryptoServiceProvider().GetBytes(data)
            Return Convert.ToBase64String(data)
        End Function

        Private Shared Function IsStatusDueToBadPassword(ByVal status As Integer) As Boolean
            Return (((status >= 2) AndAlso (status <= 6)) OrElse (status = 99))
        End Function

        Private Shared Function GetExceptionText(ByVal status As Integer) As String
            Dim exceptionText As String

            Select Case status
                Case 0
                    Return String.Empty
                Case 1
                    exceptionText = "The user was not found."
                Case 2
                    exceptionText = "The password supplied is wrong."
                Case 3
                    exceptionText = "The password-answer supplied is wrong."
                Case 4
                    exceptionText = "The password supplied is invalid.  Passwords must conform to the password strength requirements configured for the default provider."
                Case 5
                    exceptionText = "The password-question supplied is invalid.  Note that the current provider configuration requires a valid password question and answer.  As a result, a CreateUser overload that accepts question and answer parameters must also be used."
                Case 6
                    exceptionText = "The password-answer supplied is invalid."
                Case 7
                    exceptionText = "The E-mail supplied is invalid."
                Case 99
                    exceptionText = "The user account has been locked out."
                Case Else
                    exceptionText = "The Provider encountered an unknown error."
            End Select

            Return exceptionText
        End Function

        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")>
        Private Shared Function GetDBConnectionForMembership() As SqliteConnection
            If System.Web.HttpContext.Current IsNot Nothing Then
                Dim tran As SqliteTransaction = CType(System.Web.HttpContext.Current.Items(_httpTransactionId), SqliteTransaction)
                If (tran IsNot Nothing) AndAlso (String.Equals(tran.Connection.ConnectionString, _connectionString)) Then Return tran.Connection
            End If

            Return New SqliteConnection(_connectionString)
        End Function

        Private Shared Function IsTransactionInProgress() As Boolean
            If System.Web.HttpContext.Current Is Nothing Then Return False
            Dim tran As SqliteTransaction = CType(System.Web.HttpContext.Current.Items(_httpTransactionId), SqliteTransaction)

            If (tran IsNot Nothing) AndAlso (String.Equals(tran.Connection.ConnectionString, _connectionString)) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class

    Friend Class SecUtility
        Friend Shared Sub CheckParameter(ByRef param As String, ByVal checkForNull As Boolean, ByVal checkIfEmpty As Boolean, ByVal checkForCommas As Boolean, ByVal maxSize As Integer, ByVal paramName As String)
            If param Is Nothing Then

                If checkForNull Then
                    Throw New ArgumentNullException(paramName)
                End If
            Else
                param = param.Trim()

                If checkIfEmpty AndAlso (param.Length < 1) Then
                    Throw New ArgumentException(String.Format("The parameter '{0}' must not be empty.", paramName), paramName)
                End If

                If (maxSize > 0) AndAlso (param.Length > maxSize) Then
                    Throw New ArgumentException(String.Format("The parameter '{0}' is too long: it must not exceed {1} chars in length.", paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName)
                End If

                If checkForCommas AndAlso param.Contains(",") Then
                    Throw New ArgumentException(String.Format("The parameter '{0}' must not contain commas.", paramName), paramName)
                End If
            End If
        End Sub

        Friend Shared Function ValidateParameter(ByRef param As String, ByVal checkForNull As Boolean, ByVal checkIfEmpty As Boolean, ByVal checkForCommas As Boolean, ByVal maxSize As Integer) As Boolean
            If param Is Nothing Then
                Return Not checkForNull
            End If

            param = param.Trim()
            Return (((Not checkIfEmpty OrElse (param.Length >= 1)) AndAlso ((maxSize <= 0) OrElse (param.Length <= maxSize))) AndAlso (Not checkForCommas OrElse Not param.Contains(",")))
        End Function
    End Class
End Namespace
