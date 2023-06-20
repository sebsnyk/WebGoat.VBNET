Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Data
Imports System.Web.Security

Namespace OWASP.WebGoat.NET.App_Code
    Public Class Encoder
        'use for encryption
        'encryption methods taken from: http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net
        Private Shared _salt As Byte() = Encoding.ASCII.GetBytes("o6806642kbM7c5")

        ''' <summary>
        ''' Encrypt the given string using AES.  The string can be decrypted using 
        ''' DecryptStringAES().  The sharedSecret parameters must match.
        ''' </summary>
        ''' <param name="plainText">The text to encrypt.</param>
        ''' <param name="sharedSecret">A password used to generate a key for encryption.</param>
        Public Shared Function EncryptStringAES(ByVal plainText As String, ByVal sharedSecret As String) As String
            If String.IsNullOrEmpty(plainText) Then
                Throw New ArgumentNullException("plainText")
            End If
            If String.IsNullOrEmpty(sharedSecret) Then
                Throw New ArgumentNullException("sharedSecret")
            End If

            Dim outStr As String = Nothing ' Encrypted string to return
            Dim aesAlg As RijndaelManaged = Nothing ' RijndaelManaged object used to encrypt the data.

            Try
                ' generate the key from the shared secret and the salt
                Dim key As New Rfc2898DeriveBytes(sharedSecret, _salt)

                ' Create a RijndaelManaged object
                ' with the specified key and IV.
                aesAlg = New RijndaelManaged()
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8)
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8)

                ' Create a decrytor to perform the stream transform.
                Dim encryptor As ICryptoTransform = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV)

                ' Create the streams used for encryption.
                Using msEncrypt As New MemoryStream()
                    Using csEncrypt As New CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)
                        Using swEncrypt As New StreamWriter(csEncrypt)

                            'Write all data to the stream.
                            swEncrypt.Write(plainText)
                        End Using
                    End Using
                    outStr = Convert.ToBase64String(msEncrypt.ToArray())
                End Using
            Finally
                ' Clear the RijndaelManaged object.
                If aesAlg IsNot Nothing Then
                    aesAlg.Clear()
                End If
            End Try

            ' Return the encrypted bytes from the memory stream.
            Return outStr
        End Function

        ''' <summary>
        ''' Decrypt the given string.  Assumes the string was encrypted using 
        ''' EncryptStringAES(), using an identical sharedSecret.
        ''' </summary>
        ''' <param name="cipherText">The text to decrypt.</param>
        ''' <param name="sharedSecret">A password used to generate a key for decryption.</param>
        Public Shared Function DecryptStringAES(ByVal cipherText As String, ByVal sharedSecret As String) As String
            If String.IsNullOrEmpty(cipherText) Then
                Throw New ArgumentNullException("cipherText")
            End If
            If String.IsNullOrEmpty(sharedSecret) Then
                Throw New ArgumentNullException("sharedSecret")
            End If

            ' Declare the RijndaelManaged object
            ' used to decrypt the data.
            Dim aesAlg As RijndaelManaged = Nothing

            ' Declare the string used to hold
            ' the decrypted text.
            Dim plaintext As String = Nothing

            Try
                ' generate the key from the shared secret and the salt
                Dim key As New Rfc2898DeriveBytes(sharedSecret, _salt)

                ' Create a RijndaelManaged object
                ' with the specified key and IV.
                aesAlg = New RijndaelManaged()
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8)
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8)

                ' Create a decrytor to perform the stream transform.
                Dim decryptor As ICryptoTransform = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV)
                ' Create the streams used for decryption.                
                Dim bytes As Byte() = Convert.FromBase64String(cipherText)
                Using msDecrypt As New MemoryStream(bytes)
                    Using csDecrypt As New CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)
                        Using srDecrypt As New StreamReader(csDecrypt)

                            ' Read the decrypted bytes from the decrypting stream
                            ' and place them in a string.
                            plaintext = srDecrypt.ReadToEnd()
                        End Using
                    End Using
                End Using
            Finally
                ' Clear the RijndaelManaged object.
                If aesAlg IsNot Nothing Then
                    aesAlg.Clear()
                End If
            End Try

            Return plaintext
        End Function

        ''' <summary>
        ''' returns an base64 encoded string
        ''' </summary>
        ''' <param name="s">string to encode</param>
        ''' <returns></returns>
        Public Shared Function Encode(ByVal s As String) As String
            Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(s)
            Dim output As String = System.Convert.ToBase64String(bytes)
            Return output
        End Function

        ''' <summary>
        ''' Converts a string from Base64
        ''' </summary>
        ''' <param name="s">Base64 encoded string</param>
        ''' <returns></returns>
        Public Shared Function Decode(ByVal s As String) As String
            Dim bytes As Byte() = System.Convert.FromBase64String(s)
            Dim output As String = System.Text.Encoding.UTF8.GetString(bytes)
            Return output
        End Function


        ''' <summary>
        ''' From http://weblogs.asp.net/navaidakhtar/archive/2008/07/08/converting-data-table-dataset-into-json-string.aspx
        ''' </summary>
        ''' <param name="dt"></param>
        ''' <returns>string</returns>
        Public Shared Function ToJSONString(ByVal dt As DataTable) As String
            Dim StrDc As String() = New String(dt.Columns.Count - 1) {}

            Dim HeadStr As String = String.Empty
            For i As Integer = 0 To dt.Columns.Count - 1

                StrDc(i) = dt.Columns(i).Caption
                HeadStr += """" & StrDc(i) & """ : """ & StrDc(i) & i.ToString() & "¾" & ""","

            Next

            HeadStr = HeadStr.Substring(0, HeadStr.Length - 1)
            Dim Sb As New StringBuilder()

            Sb.Append("{" & """" & dt.TableName & """ : [")
            For i As Integer = 0 To dt.Rows.Count - 1

                Dim TempStr As String = HeadStr

                Sb.Append("{")
                For j As Integer = 0 To dt.Columns.Count - 1

                    TempStr = TempStr.Replace(dt.Columns(j).ToString & j.ToString() & "¾", dt.Rows(i)(j).ToString())

                Next
                Sb.Append(TempStr & "},")

            Next
            Sb = New StringBuilder(Sb.ToString().Substring(0, Sb.ToString().Length - 1))

            Sb.Append("]}")
            Return Sb.ToString()
        End Function

        Public Shared Function ToJSONSAutocompleteString(ByVal query As String, ByVal dt As DataTable) As String
            Dim badvalues As Char() = {"["c, "]"c, "{"c, "}"c}

            For Each c As Char In badvalues
                query = query.Replace(c, "#"c)
            Next

            Dim sb As New StringBuilder()

            sb.Append("{\nquery:'" & query & "',\n")
            sb.Append("suggestions:[")

            For i As Integer = 0 To dt.Rows.Count - 1
                Dim row As DataRow = dt.Rows(i)
                Dim email As String = row(0).ToString()
                sb.Append("'" & email & "',")
            Next

            sb = New StringBuilder(sb.ToString().Substring(0, sb.ToString().Length - 1))
            sb.Append("],\n")
            sb.Append("data:" & sb.ToString().Substring(sb.ToString().IndexOf("["c), (sb.ToString().LastIndexOf("]"c) - sb.ToString().IndexOf("["c)) + 1) & "\n}")

            Return sb.ToString()
        End Function

        Public Function EncodeTicket(ByVal token As String) As String
            Dim ticket As New FormsAuthenticationTicket(
                1, 'version 
                token, 'token 
                DateTime.Now, 'issueDate
                DateTime.Now.AddDays(14), 'expireDate 
                True, 'isPersistent
                "customer", 'userData (customer role)
                FormsAuthentication.FormsCookiePath 'cookiePath
            )

            Return FormsAuthentication.Encrypt(ticket) 'encrypt the ticket
        End Function

    End Class
End Namespace