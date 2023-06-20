Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Partial Public Class OrderDetails
        <Key>
        <Column(Order := 0)>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property orderNumber As Long

        <Key>
        <Column(Order := 1)>
        <StringLength(15)>
        Public Property productCode As String

        Public Property quantityOrdered As Long

        <Column(TypeName := "real")>
        Public Property priceEach As Double

        Public Property orderLineNumber As Short
    End Class
End Namespace