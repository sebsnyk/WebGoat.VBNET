﻿Imports System
Imports System.Web.UI

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class Invoice
        Inherits Page
        
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            lblFullName.Text = Session("buy_full_name").ToString()
            lblCountry.Text = Session("buy_country").ToString()
            lblAddress.Text = Session("buy_address").ToString()
            lblPartNo.Text = Session("buy_product_id").ToString()
            lblDescription.Text = Session("buy_product_description").ToString()
            lblPrice.Text = Session("buy_product_price").ToString()
            lblSubTotal.Text = Session("buy_product_price").ToString()
            lblShippingRate.Text = Session("buy_shipping_price").ToString()
            lblTotal.Text = Session("buy_total_price").ToString()
        End Sub
    End Class
End Namespace