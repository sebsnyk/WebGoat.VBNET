Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Partial Public Class Customers
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property customerNumber As Long

        <Required>
        <StringLength(50)>
        Public Property customerName As String

        <StringLength(100)>
        Public Property logoFileName As String

        <Required>
        <StringLength(50)>
        Public Property contactLastName As String

        <Required>
        <StringLength(50)>
        Public Property contactFirstName As String

        <Required>
        <StringLength(50)>
        Public Property phone As String

        <Required>
        <StringLength(50)>
        Public Property addressLine1 As String

        <StringLength(50)>
        Public Property addressLine2 As String

        <Required>
        <StringLength(50)>
        Public Property city As String

        <StringLength(50)>
        Public Property state As String

        <StringLength(15)>
        Public Property postalCode As String

        <Required>
        <StringLength(50)>
        Public Property country As String

        Public Property salesRepEmployeeNumber As Long?

        <Column(TypeName := "real")>
        Public Property creditLimit As Double?
    End Class
End Namespace