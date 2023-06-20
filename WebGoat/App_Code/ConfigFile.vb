Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO

Namespace OWASP.WebGoat.NET.App_Code
    Public Class ConfigFile
        Private _filePath As String

        Private _settings As IDictionary(Of String, String) = New Dictionary(Of String, String)()
        Private _settingComments As IDictionary(Of String, String) = New Dictionary(Of String, String)()

        Private _encoding As New UTF8Encoding()

        Private Const SPLIT_CHAR As Char = "="c

        Public Sub New(fileName As String)
            _filePath = fileName
        End Sub

        'TODO: Obviously no checks for problems, so when you get time do it like bhudda.
        Public Sub Load()
            Dim comment As String = String.Empty

            'It's all or nothing here buddy.
            For Each line As String In File.ReadAllLines(_filePath)

                If line.Length = 0 Then
                    Continue For
                End If

                If line(0) = "#"c Then
                    comment = line
                    Continue For
                End If

                Dim tokens() As String = line.Split(SPLIT_CHAR)

                If tokens.Length >= 2 Then
                    Dim key As String = tokens(0).ToLower()
                    _settings(key) = tokens(1)

                    If Not String.IsNullOrEmpty(comment) Then
                        _settingComments(key) = comment
                    End If
                End If

                comment = String.Empty
            Next
        End Sub

        Public Sub Save()
            Using stream As FileStream = File.Create(_filePath)
                Dim data As Byte() = ToByteArray()

                stream.Write(data, 0, data.Length)
            End Using
        End Sub

        Private Function ToByteArray() As Byte()
            Dim builder As New StringBuilder()

            For Each pair In _settings
                If _settingComments.ContainsKey(pair.Key) Then
                    builder.Append(_settingComments(pair.Key))
                    builder.AppendLine()
                End If

                builder.AppendFormat("{0}={1}", pair.Key, pair.Value)
                builder.AppendLine()
            Next

            Return _encoding.GetBytes(builder.ToString())
        End Function

        Public Function [Get](key As String) As String
            key = key.ToLower()

            If _settings.ContainsKey(key) Then
                Return _settings(key)
            End If

            Return String.Empty
        End Function

        Public Sub [Set](key As String, value As String)
            _settings(key.ToLower()) = value
        End Sub

        Public Sub Remove(key As String)
            _settings.Remove(key.ToLower())
        End Sub
    End Class
End Namespace