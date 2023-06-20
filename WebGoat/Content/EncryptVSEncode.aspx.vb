Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Security.Cryptography
Imports System.Drawing
Imports OWASP.WebGoat.NET.App_Code
Imports System.Text

Namespace OWASP.WebGoat.NET
    Partial Public Class EncryptVSEncode
        Inherits System.Web.UI.Page

        Public Property Password() As String

        Private Enum WG_Hash
            Sha1 = 1
            Sha256
        End Enum

        Private hardCodedKey As String = "key"

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            Password = "123456"
        End Sub

        Protected Sub btnGO_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim secret As String = txtString.Text
            Dim key As String = If(String.IsNullOrEmpty(txtPassword.Text), hardCodedKey, txtPassword.Text)

            Dim t As New Table()
            t.Width = New Unit("100%")

            t.Rows.Add(MakeRow("Custom Crypto", CustomCryptoEncrypt(secret)))
            t.Rows.Add(MakeRow("URL Encoded:", Server.UrlEncode(secret)))
            t.Rows.Add(MakeRow("Base64 Encoded:", Base64(secret)))
            t.Rows.Add(MakeRow("SHA1 Hashed:", SHA(secret, WG_Hash.Sha1)))
            t.Rows.Add(MakeRow("SHA256 Hashed:", SHA(secret, WG_Hash.Sha256)))
            t.Rows.Add(MakeRow("Rijndael Encrypted: ", Encypt(secret, key), Color.LightGreen))

            Dim cph As ContentPlaceHolder = CType(Me.Master.FindControl("BodyContentPlaceholder"), ContentPlaceHolder)
            cph.Controls.Add(New LiteralControl("<p/>"))
            cph.Controls.Add(t)

        End Sub

        Private Function MakeRow(ByVal label As String, ByVal val As String) As TableRow
            Dim row As New TableRow()

            Dim t1 As New TableCell()
            t1.Text = label
            row.Cells.Add(t1)

            Dim t2 As New TableCell()
            t2.Text = val
            row.Cells.Add(t2)
            Return row
        End Function

        Private Function MakeRow(ByVal label As String, ByVal val As String, ByVal color As Color) As TableRow
            Dim row As New TableRow()
            row.BackColor = color

            Dim t1 As New TableCell()
            t1.Text = label
            row.Cells.Add(t1)

            Dim t2 As New TableCell()
            t2.Text = val
            row.Cells.Add(t2)
            Return row
        End Function

        Private Function Base64(ByVal s As String) As String
            Dim bytes As Byte() = System.Text.ASCIIEncoding.ASCII.GetBytes(s)
            Return System.Convert.ToBase64String(bytes)
        End Function

        Private Function SHA(ByVal s As String, ByVal hash As WG_Hash) As String
            Dim bytes As Byte() = System.Text.ASCIIEncoding.ASCII.GetBytes(s)
            Dim result As Byte()
            Dim sha As HashAlgorithm = Nothing

            Select Case hash
                Case WG_Hash.Sha1
                    sha = New SHA1Managed()
                Case WG_Hash.Sha256
                    sha = New SHA256Managed()
            End Select
            result = sha.ComputeHash(bytes)
            Return System.Convert.ToBase64String(result)
        End Function

        Private Function Encypt(ByVal s As String, ByVal key As String) As String
            Dim result As String = OWASP.WebGoat.NET.App_Code.Encoder.EncryptStringAES(s, key)
            Return result
        End Function

        Private Function CustomCryptoEncrypt(ByVal s As String) As String
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(s)

            For i As Integer = 0 To bytes.Length - 1
                If i Mod 2 = 0 Then
                    bytes(i) = CByte(bytes(i) Or 2)
                Else
                    bytes(i) = CByte(bytes(i) And 2)
                End If

            Next

            Return Encoding.UTF8.GetString(bytes)
        End Function
    End Class
End Namespace