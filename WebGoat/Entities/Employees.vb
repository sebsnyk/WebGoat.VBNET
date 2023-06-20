Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Partial Public Class Employees
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property employeeNumber As Long

        <Required>
        <StringLength(50)>
        Public Property lastName As String

        <Required>
        <StringLength(50)>
        Public Property firstName As String

        <Required>
        <StringLength(10)>
        Public Property extension As String

        <Required>
        <StringLength(100)>
        Public Property email As String

        <Required>
        <StringLength(10)>
        Public Property officeCode As String

        Public Property reportsTo As Long?

        <Required>
        <StringLength(50)>
        Public Property jobTitle As String
    End Class
End Namespace