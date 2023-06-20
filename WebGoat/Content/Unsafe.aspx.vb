Imports System
Imports System.Text

Namespace OWASP.WebGoat.NET.Content
    Public Partial Class Unsafe
        Inherits System.Web.UI.Page

        Public Sub Page_Load(ByVal sender As Object, ByVal args As EventArgs)
        End Sub

        Public Sub btnReverse_Click(ByVal sender As Object, ByVal args As EventArgs)
            Const msg As String = "passwor"
            Const INPUT_LEN As Integer = 256
            Dim fixedChar(INPUT_LEN - 1) As Char

            For i As Integer = 0 To fixedChar.Length - 1
                fixedChar(i) = ControlChars.NullChar
            Next

            Dim txtBoxMsgText As String = txtBoxMsg.Text
            Dim revLine As New StringBuilder()
            Dim lineLen As Integer = txtBoxMsgText.Length

            For i As Integer = 0 To lineLen - 1
                revLine.Insert(i, txtBoxMsgText(lineLen - i - 1))
            Next

            lblReverse.Text = String.Empty
            For Each ch As Char In revLine.ToString()
                If ch <> ControlChars.NullChar Then
                    lblReverse.Text += ch
                Else
                    Exit For
                End If
            Next
        End Sub
    End Class
End Namespace