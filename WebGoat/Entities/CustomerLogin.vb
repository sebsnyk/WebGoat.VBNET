Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    <Table("CustomerLogin")>
    Public Partial Class CustomerLogin
        <Key>
        <StringLength(100)>
        Public Property email As String

        Public Property customerNumber As Long

        <Required>
        <StringLength(40)>
        Public Property password As String

        Public Property question_id As Short?

        <StringLength(50)>
        Public Property answer As String
    End Class
End Namespace