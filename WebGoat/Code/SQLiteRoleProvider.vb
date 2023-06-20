Imports System
Imports System.Collections.Specialized
Imports System.Configuration
Imports System.Configuration.Provider
Imports System.Data
Imports Mono.Data.Sqlite
Imports System.Web.Security

Namespace TechInfoSystems.Data.SQLite

    Public NotInheritable Class SQLiteRoleProvider
        Inherits RoleProvider

        #Region "Private Fields"

        Private Const HTTP_TRANSACTION_ID As String = "SQLiteTran"
        Private Const APP_TB_NAME As String = "[aspnet_Applications]"
        Private Const ROLE_TB_NAME As String = "[aspnet_Roles]"
        Private Const USER_TB_NAME As String = "[aspnet_Users]"
        Private Const USERS_IN_ROLES_TB_NAME As String = "[aspnet_UsersInRoles]"
        Private Const MAX_USERNAME_LENGTH As Integer = 256
        Private Const MAX_ROLENAME_LENGTH As Integer = 256
        Private Const MAX_APPLICATION_NAME_LENGTH As Integer = 256
        Private Shared _applicationId As String
        Private Shared _applicationName As String
        Private Shared _membershipApplicationId As String
        Private Shared _membershipApplicationName As String
        Private Shared _connectionString As String

        #End Region

        #Region "Public Properties"

        Public Overrides Property ApplicationName As String
            Get
                Return _applicationName
            End Get
            Set(ByVal value As String)
                If value.Length > MAX_APPLICATION_NAME_LENGTH Then
                    Throw New ProviderException(String.Format("SQLiteRoleProvider error: applicationName must be less than or equal to {0} characters.", MAX_APPLICATION_NAME_LENGTH))
                End If
                _applicationName = value
                _applicationId = GetApplicationId(_applicationName)
            End Set
        End Property

        Public Shared Property MembershipApplicationName As String
            Get
                Return _membershipApplicationName
            End Get
            Set(ByVal value As String)
                If value.Length > MAX_APPLICATION_NAME_LENGTH Then
                    Throw New ProviderException(String.Format("SQLiteRoleProvider error: membershipApplicationName must be less than or equal to {0} characters.", MAX_APPLICATION_NAME_LENGTH))
                End If
                _membershipApplicationName = value
                _membershipApplicationId = (If((_applicationName = _membershipApplicationName), _applicationId, GetApplicationId(_membershipApplicationName)))
            End Set
        End Property

        #End Region

        #Region "Public Methods"

        Public Overrides Sub Initialize(ByVal name As String, ByVal config As NameValueCollection)
            ' Initialize values from web.config.
            If config Is Nothing Then
                Throw New ArgumentNullException("config")
            End If

            If name Is Nothing OrElse name.Length = 0 Then
                name = "SQLiteRoleProvider"
            End If

            If String.IsNullOrEmpty(config("description")) Then
                config.Remove("description")
                config.Add("description", "SQLite Role provider")
            End If

            ' Initialize the abstract base class.
            MyBase.Initialize(name, config)

            ' Initialize SqliteConnection.
            Dim connectionStringSettings As ConnectionStringSettings = ConfigurationManager.ConnectionStrings(config("connectionStringName"))

            If connectionStringSettings Is Nothing OrElse connectionStringSettings.ConnectionString.Trim() = "" Then
                Throw New ProviderException("Connection string is empty for SQLiteRoleProvider. Check the web configuration file (web.config).")
            End If

            _connectionString = connectionStringSettings.ConnectionString

            ' Get application name
            If config("applicationName") Is Nothing OrElse config("applicationName").Trim() = "" Then
                Me.ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
            Else
                Me.ApplicationName = config("applicationName")
            End If

            ' Get Membership application name
            If config("membershipApplicationName") Is Nothing OrElse config("membershipApplicationName").Trim() = "" Then
                MembershipApplicationName = ApplicationName
            Else
                MembershipApplicationName = config("membershipApplicationName")
            End If

            ' Check for invalid parameters in the config
            config.Remove("connectionStringName")
            config.Remove("applicationName")
            config.Remove("membershipApplicationName")
            config.Remove("name")

            If config.Count > 0 Then
                Dim key As String = config.GetKey(0)
                If Not String.IsNullOrEmpty(key) Then
                    Throw New ProviderException(String.Concat("SQLiteRoleProvider configuration error: Unrecognized attribute: ", key))
                End If
            End If

            ' Verify a record exists in the application table.
            VerifyApplication()
        End Sub

        Public Overrides Sub AddUsersToRoles(ByVal usernames() As String, ByVal roleNames() As String)
            For Each roleName As String In roleNames
                If Not RoleExists(roleName) Then
                    Throw New ProviderException("Role name not found.")
                End If
            Next

            For Each username As String In usernames
                If username.IndexOf(",") > 0 Then
                    Throw New ArgumentException("User names cannot contain commas.")
                End If

                For Each roleName As String In roleNames
                    If IsUserInRole(username, roleName) Then
                        Throw New ProviderException("User is already in role.")
                    End If
                Next
            Next

            Dim tran As SqliteTransaction = Nothing
            Dim cn As SqliteConnection = GetDbConnectionForRole()
            Try
                If cn.State = ConnectionState.Closed Then
                    cn.Open()
                End If

                If Not IsTransactionInProgress() Then
                    tran = cn.BeginTransaction()
                End If

                Using cmd As SqliteCommand = cn.CreateCommand()
                    cmd.CommandText = "INSERT INTO " & USERS_IN_ROLES_TB_NAME _
                        & " (UserId, RoleId)" _
                        & " SELECT u.UserId, r.RoleId" _
                        & " FROM " & USER_TB_NAME & " u, " & ROLE_TB_NAME & " r" _
                        & " WHERE (u.LoweredUsername = $Username) AND (u.ApplicationId = $MembershipApplicationId)" _
                        & " AND (r.LoweredRoleName = $RoleName) AND (r.ApplicationId = $ApplicationId)"

                    Dim userParm As SqliteParameter = cmd.Parameters.Add("$Username", DbType.String, MAX_USERNAME_LENGTH)
                    Dim roleParm As SqliteParameter = cmd.Parameters.Add("$RoleName", DbType.String, MAX_ROLENAME_LENGTH)
                    cmd.Parameters.AddWithValue("$MembershipApplicationId", _membershipApplicationId)
                    cmd.Parameters.AddWithValue("$ApplicationId", _applicationId)

                    For Each username As String In usernames
                        For Each roleName As String In roleNames
                            userParm.Value = username.ToLowerInvariant()
                            roleParm.Value = roleName.ToLowerInvariant()
                            cmd.ExecuteNonQuery()
                        Next
                    Next

                    ' Commit the transaction if it's the one we created in this method.
                    If tran IsNot Nothing Then
                        tran.Commit()
                    End If
                End Using
            Catch
                If tran IsNot Nothing Then
                    Try
                        tran.Rollback()
                    Catch ex As SqliteException
                    End Try
                End If
                Throw
            Finally
                If tran IsNot Nothing Then
                    tran.Dispose()
                End If

                If Not IsTransactionInProgress() Then
                    cn.Dispose()
                End If
            End Try
        End Sub

        Public Overrides Sub CreateRole(ByVal roleName As String)
            If roleName.IndexOf(",") > 0 Then
                Throw New ArgumentException("Role names cannot contain commas.")
            End If

            If RoleExists(roleName) Then
                Throw New ProviderException("Role name already exists.")
            End If

            If Not SecUtility.ValidateParameter( roleName, True, True, False, MAX_ROLENAME_LENGTH) Then
                Throw New ProviderException (String.Format ("The role name is too long: it must not exceed {0} chars in length.", MAX_ROLENAME_LENGTH))