Imports System
Imports System.Web
Imports System.Data
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.IO
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Public Partial Class MainPage
        Inherits System.Web.UI.Page

        'TODO: Add "welcome back " + name;
        'TODO: pending orders?
        'TODO: Add "WebGoat Coins Info Center"
        'TODO: Take out monthly special, add "hear what our customers are saying" - with the latest comments.  Add date field to comments??

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            labelUpload.Visible = False
            If Request.Cookies("customerNumber") IsNot Nothing Then
                Dim customerNumber As String = Request.Cookies("customerNumber").Value

                Dim ds As DataSet = du.GetCustomerDetails(customerNumber)
                Dim row As DataRow = ds.Tables(0).Rows(0) 'customer row

                Image1.ImageUrl = "images/logos/" + row("logoFileName").ToString()

                For Each col As DataColumn In ds.Tables(0).Columns
                    Dim tablerow As New TableRow()
                    tablerow.ID = col.ColumnName.ToString()

                    Dim cell1 As New TableCell()
                    Dim cell2 As New TableCell()
                    cell1.Text = col.ColumnName.ToString()
                    cell2.Text = row(col).ToString()
                    
                    tablerow.Cells.Add(cell1)
                    tablerow.Cells.Add(cell2)
                    
                    CustomerTable.Rows.Add(tablerow)
                Next
            End If
        End Sub

        Protected Sub btnUpload_Click(sender As Object, e As EventArgs)
            If FileUpload1.HasFile Then
                Try
                    Dim filename As String = Path.GetFileName(FileUpload1.FileName)
                    FileUpload1.SaveAs(Server.MapPath("~/WebGoatCoins/uploads/") + filename)
                    
                Catch ex As Exception
                    labelUpload.Text = "Upload Failed: " + ex.Message
                Finally
                    labelUpload.Visible = True
                End Try
            End If
        End Sub
    End Class
End Namespace