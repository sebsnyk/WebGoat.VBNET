Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class Catalog
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim ds As DataSet = du.GetProductsAndCategories()

            For Each catRow As DataRow In ds.Tables("categories").Rows
                lblOutput.Text &= "<p/><h2 class='title-regular-2 clearfix'>Category: " & catRow("catName").ToString() & "</h2><hr/><p/>" & vbLf
                For Each prodRow As DataRow In catRow.GetChildRows("cat_prods")
                    lblOutput.Text &= "<div class='product' align='center'>" & vbLf
                    lblOutput.Text &= "<img src='./images/products/" & prodRow(3) & "'/><br/>" & vbLf
                    lblOutput.Text &= "" & prodRow(1) & "<br/>" & vbLf
                    lblOutput.Text &= "<a href=""ProductDetails.aspx?productNumber=" & prodRow(0).ToString() & """><br/>" & vbLf
                    lblOutput.Text &= "<img src=""../resources/images/moreinfo1.png"" onmouseover=""this.src='../resources/images/moreinfo2.png';"" onmouseout=""this.src='../resources/images/moreinfo1.png';"" />" & vbLf
                    lblOutput.Text &= "</a>" & vbLf
                    lblOutput.Text &= "<a href=""Buy.aspx?id=" & prodRow(0) & """><br/>" & vbLf
                    lblOutput.Text &= "<img src=""../resources/images/buy-now-button.png"" />" & vbLf
                    lblOutput.Text &= "</a>" & vbLf
                    lblOutput.Text &= "</div>" & vbLf
                Next
            Next
        End Sub
    End Class
End Namespace