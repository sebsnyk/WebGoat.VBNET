Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports System.IO
Imports System.Configuration
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Public Partial Class RebuildDatabase
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            Dim configFile As ConfigFile = Settings.CurrentConfigFile

            If Not Page.IsPostBack Then
                dropDownDataProvider.Text = configFile.Get(DbConstants.KEY_DB_TYPE)
                txtClientExecutable.Text = configFile.Get(DbConstants.KEY_CLIENT_EXEC)
                txtFilePath.Text = configFile.Get(DbConstants.KEY_FILE_NAME)
                txtServer.Text = configFile.Get(DbConstants.KEY_HOST)
                txtPort.Text = configFile.Get(DbConstants.KEY_PORT)
                txtDatabase.Text = configFile.Get(DbConstants.KEY_DATABASE)
                txtUserName.Text = configFile.Get(DbConstants.KEY_UID)
                txtPassword.Text = configFile.Get(DbConstants.KEY_PWD)
            End If

            PanelSuccess.Visible = False
            PanelError.Visible = False

            PanelRebuildSuccess.Visible = False
            PanelRebuildFailure.Visible = False
        End Sub

        Protected Sub btnTest_Click(ByVal sender As Object, ByVal e As EventArgs)
            lblOutput.Text = If(Settings.CurrentDbProvider.TestConnection(), "Works!", "Problem")
        End Sub

        Protected Sub btnTestConfiguration_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim configFile As ConfigFile = Settings.CurrentConfigFile

            UpdateConfigFile(configFile)

            Settings.CurrentDbProvider = DbProviderFactory.Create(configFile)

            If Settings.CurrentDbProvider.TestConnection() Then
                labelSuccess.Text = "Connection to Database Successful!"
                PanelSuccess.Visible = True
                Session("DBConfigured") = True
            Else
                labelError.Text = "Error testing database. Please see logs."
                PanelError.Visible = True
                Session("DBConfigured") = Nothing
            End If
        End Sub

        Protected Sub btnRebuildDatabase_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim configFile As ConfigFile = Settings.CurrentConfigFile

            UpdateConfigFile(configFile)

            Settings.CurrentDbProvider = DbProviderFactory.Create(configFile)
            Settings.CurrentDbProvider.RecreateGoatDb()

            If Settings.CurrentDbProvider.TestConnection() Then
                labelRebuildSuccess.Text = "Database Rebuild Successful!"
                PanelRebuildSuccess.Visible = True
                Session("DBConfigured") = True
            Else
                labelRebuildFailure.Text = "Error rebuilding database. Please see logs."
                PanelRebuildFailure.Visible = True
                Session("DBConfigured") = Nothing
            End If
        End Sub

        Private Sub UpdateConfigFile(ByVal configFile As ConfigFile)
            If String.IsNullOrEmpty(txtServer.Text) Then
                configFile.Remove(DbConstants.KEY_HOST)
            Else
                configFile.Set(DbConstants.KEY_HOST, txtServer.Text)
            End If

            If String.IsNullOrEmpty(txtFilePath.Text) Then
                configFile.Remove(DbConstants.KEY_FILE_NAME)
            Else
                configFile.Set(DbConstants.KEY_FILE_NAME, txtFilePath.Text)
            End If

            If String.IsNullOrEmpty(dropDownDataProvider.Text) Then
                configFile.Remove(DbConstants.KEY_DB_TYPE)
            Else
                configFile.Set(DbConstants.KEY_DB_TYPE, dropDownDataProvider.Text)
            End If

            If String.IsNullOrEmpty(txtPort.Text) Then
                configFile.Remove(DbConstants.KEY_PORT)
            Else
                configFile.Set(DbConstants.KEY_PORT, txtPort.Text)
            End If

            If String.IsNullOrEmpty(txtClientExecutable.Text) Then
                configFile.Remove(DbConstants.KEY_CLIENT_EXEC)
            Else
                configFile.Set(DbConstants.KEY_CLIENT_EXEC, txtClientExecutable.Text)
            End If

            If String.IsNullOrEmpty(txtDatabase.Text) Then
                configFile.Remove(DbConstants.KEY_DATABASE)
            Else
                configFile.Set(DbConstants.KEY_DATABASE, txtDatabase.Text)
            End If

            If String.IsNullOrEmpty(txtUserName.Text) Then
                configFile.Remove(DbConstants.KEY_UID)
            Else
                configFile.Set(DbConstants.KEY_UID, txtUserName.Text)
            End If

            If String.IsNullOrEmpty(txtPassword.Text) Then
                configFile.Remove(DbConstants.KEY_PWD)
            Else
                configFile.Set(DbConstants.KEY_PWD, txtPassword.Text)
            End If

            configFile.Save()
        End Sub
    End Class
End Namespace