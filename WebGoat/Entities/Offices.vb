Imports System.ComponentModel.DataAnnotations

Namespace OWASP.WebGoat.NET.Entities
    Public Partial Class Offices
        <Key>
        <StringLength(10)>
        Public Property officeCode As String

        <Required>
        <StringLength(50)>
        Public Property city As String

        <Required>
        <StringLength(50)>
        Public Property phone As String

        <Required>
        <StringLength(50)>
        Public Property addressLine1 As String

        <StringLength(50)>
        Public Property addressLine2 As String

        <StringLength(50)>
        Public Property state As String

        <Required>
        <StringLength(50)>
        Public Property country As String

        <Required>
        <StringLength(15)>
        Public Property postalCode As String

        <Required>
        <StringLength(10)>
        Public Property territory As String
    End Class
End Namespace