Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.IO

Namespace OWASP.WebGoat.NET
    Partial Public Class UploadPathManipulation
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs)
        End Sub

        Protected Sub btnUpload_Click(sender As Object, e As EventArgs)
            If FileUpload1.HasFile Then
                Try
                    Dim filename As String = Path.GetFileName(FileUpload1.FileName)
                    FileUpload1.SaveAs(Server.MapPath("~/WebGoatCoins/uploads/") & filename)
                    labelUpload.Text = "<div class='success' style='text-align:center'>The file " & FileUpload1.FileName & " has been saved in to the WebGoatCoins/uploads directory</div>"

                Catch ex As Exception
                    labelUpload.Text = "<div class='error' style='text-align:center'>Upload Failed: " & ex.Message & "</div>"
                Finally
                    labelUpload.Visible = True
                End Try
            End If
        End Sub
    End Class
End Namespace