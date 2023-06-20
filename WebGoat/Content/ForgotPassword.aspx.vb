Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET
    Partial Public Class ForgotPassword
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            If Not Page.IsPostBack Then
                PanelForgotPasswordStep2.Visible = False
                PanelForgotPasswordStep3.Visible = False
            End If
        End Sub

        Protected Sub ButtonCheckEmail_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim result() As String = du.GetSecurityQuestionAndAnswer(txtEmail.Text)

            If String.IsNullOrEmpty(result(0)) Then
                labelQuestion.Text = "That email address was not found in our database!"
                PanelForgotPasswordStep2.Visible = False
                PanelForgotPasswordStep3.Visible = False

                Return
            End If
            labelQuestion.Text = "Here is thequestion we have on file for you: <strong>" & result(0) & "</strong>"
            PanelForgotPasswordStep2.Visible = True
            PanelForgotPasswordStep3.Visible = False

            Dim cookie As HttpCookie = New HttpCookie("encr_sec_qu_ans")

            'encode twice for more security!
            cookie.Value = Encoder.Encode(Encoder.Encode(result(1)))

            Response.Cookies.Add(cookie)
        End Sub

        Protected Sub ButtonRecoverPassword_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim encrypted_password As String = Request.Cookies("encr_sec_qu_ans").Value.ToString()

                'decode it (twice for extra security!)
                Dim security_answer As String = Encoder.Decode(Encoder.Decode(encrypted_password))

                If security_answer.Trim().ToLower().Equals(txtAnswer.Text.Trim().ToLower()) Then
                    PanelForgotPasswordStep1.Visible = False
                    PanelForgotPasswordStep2.Visible = False
                    PanelForgotPasswordStep3.Visible = True
                    labelPassword.Text = "Security Question Challenge Successfully Completed! <br/>Your password is: " & getPassword(txtEmail.Text)
                End If
            Catch ex As Exception
                labelMessage.Text = "An unknown error occurred - Do you have cookies turned on? Further Details: " & ex.Message
            End Try
        End Sub

        Private Function getPassword(ByVal email As String) As String
            Dim password As String = du.GetPasswordByEmail(email)
            Return password
        End Function

    End Class
End Namespace