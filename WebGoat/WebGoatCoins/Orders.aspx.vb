Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports System.IO
Imports System.Collections.Specialized
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class Orders
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            Dim id As Integer
            Dim ds As DataSet
            If Request.Cookies("customerNumber") Is Nothing OrElse Not Integer.TryParse(Request.Cookies("customerNumber").Value.ToString(), id) Then
                lblOutput.Text = "Sorry, an unspecified problem regarding your Customer ID has occurred.  Are your cookies enabled?"
            Else
                ds = du.GetOrders(id)

                If Not Page.IsPostBack Then 'generate the data grid
                    GridView1.DataSource = ds.Tables(0)

                    GridView1.AutoGenerateColumns = False

                    Dim BoundFieldOrderNumber As New BoundField()
                    Dim BoundFieldStatus As New BoundField()
                    Dim BoundFieldRequiredDate As New BoundField()
                    Dim BoundFieldShippedDate As New BoundField()

                    BoundFieldOrderNumber.DataField = "orderNumber"
                    BoundFieldStatus.DataField = "status"
                    BoundFieldRequiredDate.DataField = "requiredDate"
                    BoundFieldShippedDate.DataField = "shippedDate"

                    BoundFieldOrderNumber.HeaderText = "Order Number"
                    BoundFieldStatus.HeaderText = "Status"
                    BoundFieldRequiredDate.HeaderText = "Required Date"
                    BoundFieldShippedDate.HeaderText = "Shipped Date"

                    BoundFieldRequiredDate.DataFormatString = "{0:MM/dd/yyyy}"
                    BoundFieldShippedDate.DataFormatString = "{0:MM/dd/yyyy}"

                    GridView1.Columns.Add(BoundFieldOrderNumber)
                    GridView1.Columns.Add(BoundFieldStatus)
                    GridView1.Columns.Add(BoundFieldRequiredDate)
                    GridView1.Columns.Add(BoundFieldShippedDate)

                    GridView1.DataBind()
                End If
                'check if orderNumber exists
                Dim orderNumber As String = Request("orderNumber")
                If orderNumber IsNot Nothing Then
                    Try
                        'lblOutput.Text = orderNumber;
                        Dim dsOrderDetails As DataSet = du.GetOrderDetails(Integer.Parse(orderNumber))
                        DetailsView1.DataSource = dsOrderDetails.Tables(0)
                        DetailsView1.DataBind()
                        'litOrderDetails.Visible = true;
                        PanelShowDetailSuccess.Visible = True

                        'allow customer to download image of their product
                        Dim image As String = dsOrderDetails.Tables(0).Rows(0)("productImage").ToString()
                        HyperLink1.Text = "Download Product Image"
                        HyperLink1.NavigateUrl = Request.RawUrl & "&image=images/products/" & image
                    Catch ex As Exception
                        'litOrderDetails.Text = "Error finding order number " + orderNumber + ". Details: " + ex.Message;
                        PanelShowDetailFailure.Visible = True
                        litErrorDetailMessage.Text = "Error finding order number " & orderNumber & ". Details: " & ex.Message
                    End Try
                End If

                'check if they are trying to download the image
                Dim target_image As String = Request("image")
                If target_image IsNot Nothing Then
                    Dim fi As New FileInfo(Server.MapPath(target_image))
                    lblOutput.Text = fi.FullName

                    Dim imageExtensions As New NameValueCollection()
                    imageExtensions.Add(".jpg", "image/jpeg")
                    imageExtensions.Add(".gif", "image/gif")
                    imageExtensions.Add(".png", "image/png")

                    Response.ContentType = imageExtensions.Get(fi.Extension)
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" & fi.Name)
                    Response.TransmitFile(fi.FullName)
                    Response.[End]()
                End If

            End If
        End Sub

        Protected Sub GridView1_RowDataBound(ByVal sender As Object, ByVal e As GridViewRowEventArgs)
            'make the first column a hyperlink
            If e.Row.RowType = DataControlRowType.DataRow Then
                Dim link As New HyperLink()
                link.Text = e.Row.Cells(0).Text
                link.NavigateUrl = "Orders.aspx?orderNumber=" & link.Text
                e.Row.Cells(0).Controls.Add(link)
            End If

        End Sub
    End Class
End Namespace