Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.IO

Namespace OWASP.WebGoat.NET
    Public Partial Class PathManipulation
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            'if(Request.QueryString["filename"] == null)
            '{
            Dim di As DirectoryInfo = New DirectoryInfo(Server.MapPath("~/Downloads"))
            Dim i As Integer = 0

            For Each fi As FileInfo In di.GetFiles()
                Dim HL As HyperLink = New HyperLink()
                HL.ID = "HyperLink" & i
                i += 1
                HL.Text = fi.Name
                HL.NavigateUrl = Request.FilePath & "?filename=" & fi.Name
                Dim cph As ContentPlaceHolder = DirectCast(Me.Master.FindControl("BodyContentPlaceholder"), ContentPlaceHolder)
                cph.Controls.Add(HL)
                cph.Controls.Add(New LiteralControl("<br/>"))
            Next
            '}
            'else
            '{
            Dim filename As String = Request.QueryString("filename")

            If filename IsNot Nothing Then
                Try
                    ResponseFile(Request, Response, filename, MapPath("~/Downloads/" & filename), 100)
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    lblStatus.Text = "File not found: " & filename
                End Try
            End If
            '}
        End Sub

        Public Shared Function ResponseFile(ByVal _Request As HttpRequest, ByVal _Response As HttpResponse, ByVal _fileName As String, ByVal _fullPath As String, ByVal _speed As Long) As Boolean
            Try
                Dim myFile As FileStream = New FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim br As BinaryReader = New BinaryReader(myFile)

                Try
                    _Response.AddHeader("Accept-Ranges", "bytes")
                    _Response.Buffer = False
                    Dim fileLength As Long = myFile.Length
                    Dim startBytes As Long = 0
                    Dim pack As Integer = 10240

                    If _Request.Headers("Range") IsNot Nothing Then
                        _Response.StatusCode = 206
                        Dim range() As String = _Request.Headers("Range").Split(New Char() {"="c, "-"c})
                        startBytes = Convert.ToInt64(range(1))
                    End If

                    _Response.AddHeader("Content-Length", (fileLength - startBytes).ToString())

                    If startBytes <> 0 Then
                        _Response.AddHeader("Content-Range", String.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength))
                    End If

                    _Response.AddHeader("Connection", "Keep-Alive")
                    _Response.ContentType = "application/octet-stream"
                    _Response.AddHeader("Content-Disposition", "attachment;filename=" & HttpUtility.UrlEncode(_fileName, System.Text.Encoding.UTF8))
                    br.BaseStream.Seek(startBytes, SeekOrigin.Begin)
                    Dim maxCount As Integer = CInt(Math.Floor(CDbl(fileLength - startBytes) / pack)) + 1

                    For i As Integer = 0 To maxCount - 1
                        If _Response.IsClientConnected Then
                            _Response.BinaryWrite(br.ReadBytes(pack))
                        Else
                            i = maxCount
                        End If
                    Next
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Return False
                Finally
                    br.Close()
                    myFile.Close()
                End Try
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return False
            End Try
            Return True
        End Function

    End Class
End Namespace