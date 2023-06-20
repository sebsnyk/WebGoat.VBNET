Imports System
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Public Partial Class Orders
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property orderNumber As Long

        Public Property orderDate As DateTime

        Public Property requiredDate As DateTime

        Public Property shippedDate As DateTime?

        <Required>
        <StringLength(15)>
        Public Property status As String

        <StringLength(2147483647)>
        Public Property comments As String

        Public Property customerNumber As Long
    End Class
End Namespace