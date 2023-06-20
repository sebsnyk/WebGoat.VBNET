Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Xml
Imports System.Xml.XPath

Namespace OWASP.WebGoat.NET
    Partial Public Class XPathInjection
        Inherits System.Web.UI.Page

        ' Make into actual lesson
        Private xml As String = "<?xml version=""1.0"" encoding=""ISO-8859-1""?><sales><salesperson><name>David Palmer</name><city>Portland</city><state>or</state><ssn>123-45-6789</ssn></salesperson><salesperson><name>Jimmy Jones</name><city>San Diego</city><state>ca</state><ssn>555-45-6789</ssn></salesperson><salesperson><name>Tom Anderson</name><city>New York</city><state>ny</state><ssn>444-45-6789</ssn></salesperson><salesperson><name>Billy Moses</name><city>Houston</city><state>tx</state><ssn>333-45-6789</ssn></salesperson></sales>"

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If Request.QueryString("state") IsNot Nothing Then
                FindSalesPerson(Request.QueryString("state"))
            End If
        End Sub

        Private Sub FindSalesPerson(state As String)
            Dim xDoc As New XmlDocument()
            xDoc.LoadXml(xml)
            Dim list As XmlNodeList = xDoc.SelectNodes("//salesperson[state='" + state + "']")
            If list.Count > 0 Then

            End If

        End Sub
    End Class
End Namespace