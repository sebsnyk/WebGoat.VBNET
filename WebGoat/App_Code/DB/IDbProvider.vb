Imports System
Imports System.Data

Namespace OWASP.WebGoat.NET.App_Code.DB
    Public Interface IDbProvider
        ReadOnly Property Name As String

        Function TestConnection() As Boolean

        Function GetCatalogData() As DataSet

        Function IsValidCustomerLogin(ByVal email As String, ByVal password As String) As Boolean

        Function IsAdminCustomerLogin(ByVal email As String) As Boolean

        Function CreateCustomer(ByVal name As String, ByVal email As String, ByVal password As String, ByVal isAdmin As Boolean, ByVal question As Integer, ByVal answer As String) As Boolean

        Function RecreateGoatDb() As Boolean

        Function GetCustomerEmail(ByVal customerNumber As String) As String

        Function GetCustomerDetails(ByVal customerNumber As String) As DataSet

        Function GetOffice(ByVal city As String) As DataSet

        Function GetMessages(ByVal customerLogin As String) As DataSet

        Function GetComments(ByVal productCode As String) As DataSet

        Function AddComment(ByVal productCode As String, ByVal email As String, ByVal comment As String) As String

        Function UpdateCustomerPassword(ByVal customerNumber As Integer, ByVal password As String) As String

        Function GetSecurityQuestionAndAnswer(ByVal email As String) As String()

        Function GetPasswordByEmail(ByVal email As String) As String

        Function GetUsers() As DataSet

        Function GetOrders(ByVal customerID As Integer) As DataSet

        Function GetProductDetails(ByVal productCode As String) As DataSet

        Function GetOrderDetails(ByVal orderNumber As Integer) As DataSet

        Function GetPayments(ByVal customerNumber As Integer) As DataSet

        Function GetProductsAndCategories() As DataSet

        Function GetProductsAndCategories(ByVal catNumber As Integer) As DataSet

        Function GetEmailByName(ByVal name As String) As String

        Function GetEmailByCustomerNumber(ByVal num As String) As String

        Function GetCustomerEmails(ByVal email As String) As DataSet
    End Interface
End Namespace