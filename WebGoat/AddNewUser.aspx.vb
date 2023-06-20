Imports System
Imports System.Collections
Imports System.Configuration
Imports System.Data
Imports System.Linq
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts

Namespace OWASP.WebGoat.NET
    Public Partial Class AddNewUser
        Inherits System.Web.UI.Page
        Const passwordQuestion As String = "What is your favorite color"

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            If Not Page.IsPostBack Then
                SecurityQuestion.Text = passwordQuestion
            End If
        End Sub

        Protected Sub CreateAccountButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim createStatus As MembershipCreateStatus

            Dim newUser As MembershipUser = Membership.CreateUser(Username.Text, Password.Text, Email.Text, passwordQuestion, SecurityAnswer.Text, True, createStatus)

            If newUser Is Nothing Then
                Console.WriteLine("New User is null!")
            End If

            Select Case createStatus
                Case MembershipCreateStatus.Success
                    CreateAccountResults.Text = "The user account was successfully created!"
                    Exit Select

                Case MembershipCreateStatus.DuplicateUserName
                    CreateAccountResults.Text = "There already exists a user with this username."
                    Exit Select

                Case MembershipCreateStatus.DuplicateEmail
                    CreateAccountResults.Text = "There already exists a user with this email address."
                    Exit Select

                Case MembershipCreateStatus.InvalidEmail
                    CreateAccountResults.Text = "There email address you provided in invalid."
                    Exit Select

                Case MembershipCreateStatus.InvalidAnswer
                    CreateAccountResults.Text = "There security answer was invalid."
                    Exit Select

                Case MembershipCreateStatus.InvalidPassword
                    CreateAccountResults.Text = "The password you provided is invalid. It must be seven characters long and have at least one non-alphanumeric character."
                    Exit Select

                Case Else
                    CreateAccountResults.Text = "There was an unknown error; the user account was NOT created."
                    Exit Select
            End Select
        End Sub

        Protected Sub RegisterUser_CreatingUser(ByVal sender As Object, ByVal e As LoginCancelEventArgs)
            'Dim trimmedUserName As String = RegisterUser.UserName.Trim()
            'If RegisterUser.UserName.Length <> trimmedUserName.Length Then
            '    ' Show the error message
            '    InvalidUserNameOrPasswordMessage.Text = "The username cannot contain leading or trailing spaces."
            '    InvalidUserNameOrPasswordMessage.Visible = True

            '    ' Cancel the create user workflow
            '    e.Cancel = True
            'Else
            '    ' Username is valid, make sure that the password does not contain the username
            '    If RegisterUser.Password.IndexOf(RegisterUser.UserName, StringComparison.OrdinalIgnoreCase) >= 0 Then
            '        ' Show the error message
            '        InvalidUserNameOrPasswordMessage.Text = "The username may not appear anywhere in the password."
            '        InvalidUserNameOrPasswordMessage.Visible = True

            '        ' Cancel the create user workflow
            '        e.Cancel = True
            '    End If
            'End If
        End Sub
    End Class

End Namespace