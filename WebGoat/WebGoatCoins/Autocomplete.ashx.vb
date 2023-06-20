Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Data
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    ''' <summary>
    ''' Summary description for Autocomplete
    ''' </summary>
    Public Class Autocomplete
        Implements IHttpHandler

        Private du As IDbProvider = Settings.CurrentDbProvider

        Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
            'context.Response.ContentType = "text/plain";
            'context.Response.Write("Hello World");

            Dim query As String = context.Request("query")

            Dim ds As DataSet = du.GetCustomerEmails(query)
            Dim json As String = Encoder.ToJSONSAutocompleteString(query, ds.Tables(0))

            If json IsNot Nothing AndAlso json.Length > 0 Then
                context.Response.ContentType = "text/plain"
                context.Response.Write(json)
            Else
                context.Response.ContentType = "text/plain"
                context.Response.Write("")

            End If
        End Sub

        Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
            Get
                Return False
            End Get
        End Property
    End Class
End Namespace