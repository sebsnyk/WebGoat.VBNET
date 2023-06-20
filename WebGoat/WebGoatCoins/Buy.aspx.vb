Imports System
Imports System.Data
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class Buy
        Inherits Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

            If Not Page.IsPostBack Then

                If Request("id") Is Nothing Then Throw New Exception("Not valid request")

                Dim id = Request("id")
                Dim price As Double
                Dim description As String
                Dim html = GetProductContent(id, description, price)
                Session("buy_product_id") = id
                Session("buy_product_description") = description
                Session("buy_product_price") = price
                Session("buy_product_html") = html
                lblOutputStep0.Text = html

            Else

                Select Case BuyWizard.ActiveStepIndex
                    Case 0
                        Exit Select
                    Case 1
                        Session("buy_full_name") = tbFullName.Text
                        Session("buy_address") = tbAddress.Text
                        UpdatePriceIncludeShipping()
                    Case 2
                        Exit Select
                    Case Else
                        Throw New NotImplementedException()
                End Select

            End If

        End Sub

        Protected Sub OnActiveStepChanged(ByVal sender As Object, ByVal e As EventArgs)
            
            Select Case BuyWizard.ActiveStepIndex
                Case 0
                    lblOutputStep0.Text = CType(Session("buy_product_html"), String)
                Case 1
                    tbFullName.Text = CType(Session("buy_full_name"), String)
                    tbAddress.Text = CType(Session("buy_address"), String)
                    ddlCountry.SelectedValue = CType(Session("buy_country"), String)
                    UpdatePriceIncludeShipping()
                Case 2
                    Exit Select
                Case Else
                    Throw New NotImplementedException()
            End Select

        End Sub

        Protected Sub OnFinishButtonClick(ByVal sender As Object, ByVal e As WizardNavigationEventArgs)
            Response.Redirect("~/WebGoatCoins/Invoice.aspx")
        End Sub

        Protected Sub OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
            UpdatePriceIncludeShipping()
        End Sub

        Private Sub UpdatePriceIncludeShipping()
            
            Session("buy_country") = ddlCountry.SelectedValue

            If ddlCountry.SelectedValue = "Australia" Then
                Session("buy_shipping_price") = 14.0
            Else
                Session("buy_shipping_price") = 10.0
            End If

            Session("buy_total_price") = (CType(Session("buy_product_price"), Double) + CType(Session("buy_shipping_price"), Double))
            lblShippingPrice.Text = " $" & Session("buy_shipping_price")
            lblTotalPrice.Text = " $" & Session("buy_total_price")

        End Sub

        Private Function GetProductContent(ByVal id As String, ByRef description As String, ByRef price As Double) As String

            price = 0.0
            description = String.Empty
            Dim output = String.Empty
            Dim ds = du.GetProductDetails(id)

            For Each prodRow As DataRow In ds.Tables("products").Rows
                output &= "<div class='product2' align='center'>"
                output &= "<img src='./images/products/" & prodRow("productImage") & "'/><br/>"
                output &= "<strong>" & prodRow("productName") & "</strong><br/>"
                output &= "<hr/>" & prodRow("productDescription") & "<br/>"
                output &= "<hr/><strong>Product price:</strong> $" & prodRow("buyPrice") & "<br/>"
                output &= "</div>"

                price = CDbl(prodRow("buyPrice"))
                description = CStr(prodRow("productName"))
            Next

            Return output

        End Function

    End Class

End Namespace