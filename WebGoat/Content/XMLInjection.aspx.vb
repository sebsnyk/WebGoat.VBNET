Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Xml
Imports System.Xml.XPath
Imports System.Data
Imports System.IO
Imports System.Text

Namespace OWASP.WebGoat.NET
    Partial Public Class XMLInjection
        Inherits System.Web.UI.Page

        Private users As List(Of XmlUser)

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            ReadXml()

            gvUsers.DataSource = users.ToArray()
            gvUsers.DataBind()

            If Request.QueryString("name") IsNot Nothing AndAlso Request.QueryString("email") IsNot Nothing Then
                users.Add(New XmlUser(Request.QueryString("name"), Request.QueryString("email")))
                WriteXML()
            End If
        End Sub

        Private Sub ReadXml()
            users = New List(Of XmlUser)()
            Dim doc As New XmlDocument()
            doc.Load(Server.MapPath("/App_Data/XmlInjectionUsers.xml"))
            For Each node As XmlNode In doc.ChildNodes(1).ChildNodes
                If node.Name = "user" Then
                    users.Add(New XmlUser(node.ChildNodes(0).InnerText, node.ChildNodes(1).InnerText))
                End If
            Next
        End Sub

        Private Sub WriteXML()
            Dim xml As String = "<?xml version=""1.0"" standalone=""yes""?>" & Environment.NewLine & "<users>" & Environment.NewLine
            For Each user As XmlUser In users
                xml &= "<user>" & Environment.NewLine
                xml &= "<name>" & user.Name & "</name>" & Environment.NewLine
                xml &= "<email>" & user.Email & "</email>" & Environment.NewLine
                xml &= "</user>" & Environment.NewLine
            Next
            xml &= "</users>" & Environment.NewLine

            Dim writer As New XmlTextWriter(Server.MapPath("/App_Data/XmlInjectionUsers.xml"), Encoding.UTF8)
            writer.WriteRaw(xml)
            writer.Close()
        End Sub
    End Class

    Public Class XmlUser
        Public Property Name As String
        Public Property Email As String

        Public Sub New(ByVal name As String, ByVal email As String)
            Me.Name = name
            Me.Email = email
        End Sub
    End Class
End Namespace