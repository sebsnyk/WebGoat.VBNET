Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Partial Public Class Products
        <Key>
        <StringLength(15)>
        Public Property productCode As String

        <Required>
        <StringLength(200)>
        Public Property productName As String

        Public Property catNumber As Long

        <Required>
        <StringLength(100)>
        Public Property productImage As String

        <Required>
        <StringLength(50)>
        Public Property productVendor As String

        <Required>
        <StringLength(2147483647)>
        Public Property productDescription As String

        Public Property quantityInStock As Short

        <Column(TypeName:="real")>
        Public Property buyPrice As Double

        <Column(TypeName:="real")>
        Public Property MSRP As Double
    End Class
End Namespace