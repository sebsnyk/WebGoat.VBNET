Imports System
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Public Partial Class Payments
        <Key>
        <Column(Order := 0)>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property customerNumber As Long

        <Required>
        <StringLength(50)>
        Public Property cardType As String

        <Required>
        <StringLength(50)>
        Public Property creditCardNumber As String

        Public Property verificationCode As Short

        <Required>
        <StringLength(3)>
        Public Property cardExpirationMonth As String

        <Required>
        <StringLength(5)>
        Public Property cardExpirationYear As String

        <Key>
        <Column(Order := 1)>
        <StringLength(50)>
        Public Property confirmationCode As String

        Public Property paymentDate As DateTime

        <Column(TypeName := "real")>
        Public Property amount As Double
    End Class
End Namespace