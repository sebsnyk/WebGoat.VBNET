Imports System.Data.Entity

Namespace OWASP.WebGoat.NET.Entities
    Public Partial Class CoinsDB
        Inherits DbContext
        
        Public Sub New()
            MyBase.New("name=CoinsDB")
        End Sub
        
        Public Overridable Property Categories As DbSet(Of Categories)
        Public Overridable Property Comments As DbSet(Of Comments)
        Public Overridable Property CustomerLogin As DbSet(Of CustomerLogin)
        Public Overridable Property Customers As DbSet(Of Customers)
        Public Overridable Property Employees As DbSet(Of Employees)
        Public Overridable Property Offices As DbSet(Of Offices)
        Public Overridable Property OrderDetails As DbSet(Of OrderDetails)
        Public Overridable Property Orders As DbSet(Of Orders)
        Public Overridable Property Payments As DbSet(Of Payments)
        Public Overridable Property Products As DbSet(Of Products)
        Public Overridable Property SecurityQuestions As DbSet(Of SecurityQuestions)
        
        Protected Overrides Sub OnModelCreating(modelBuilder As DbModelBuilder)
            modelBuilder.Entity(Of Categories)() _
                .Property(Function(e) e.catName) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Comments)() _
                .Property(Function(e) e.productCode) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Comments)() _
                .Property(Function(e) e.email) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of CustomerLogin)() _
                .Property(Function(e) e.email) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of CustomerLogin)() _
                .Property(Function(e) e.password) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of CustomerLogin)() _
                .Property(Function(e) e.answer) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.customerName) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.logoFileName) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.contactLastName) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.contactFirstName) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.phone) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.addressLine1) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.addressLine2) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.city) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.state) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.postalCode) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Customers)() _
                .Property(Function(e) e.country) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Employees)() _
                .Property(Function(e) e.lastName) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Employees)() _
                .Property(Function(e) e.firstName) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Employees)() _
                .Property(Function(e) e.extension) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Employees)() _
                .Property(Function(e) e.email) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Employees)() _
                .Property(Function(e) e.officeCode) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Employees)() _
                .Property(Function(e) e.jobTitle) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.officeCode) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.city) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.phone) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.addressLine1) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.addressLine2) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.state) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.country) _
                .IsUnicode(false)
                
            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.postalCode) _
                .IsUnicode(false)

            modelBuilder.Entity(Of Offices)() _
                .Property(Function(e) e.territory) _
                .IsUnicode(false)

            modelBuilder.Entity(Of OrderDetails)() _
                .Property(Function(e) e.productCode) _
                .IsUnicode(false)

            modelBuilder.Entity(Of Orders)() _
                .Property(Function(e) e.status) _
                .IsUnicode(false)

            modelBuilder.Entity(Of Payments)() _
                .Property(Function(e) e.cardType) _
                .IsUnicode(false)

            modelBuilder.Entity(Of Payments)() _
                .Property(Function(e) e.creditCardNumber) _
                .IsUnicode(false)

            modelBuilder.Entity(Of Payments)() _
                .Property(Function(e) e.cardExpirationMonth) _
                .IsUnicode(False)

            modelBuilder.Entity(Of Payments)() _
                .Property(Function(e) e.cardExpirationYear) _
                .IsUnicode(False)

            modelBuilder.Entity(Of Payments)() _
                .Property(Function(e) e.confirmationCode) _
                .IsUnicode(False)

            modelBuilder.Entity(Of Products)() _
                .Property(Function(e) e.productCode) _
                .IsUnicode(False)

            modelBuilder.Entity(Of Products)() _
                .Property(Function(e) e.productName) _
                .IsUnicode(False)

            modelBuilder.Entity(Of Products)() _
                .Property(Function(e) e.productImage) _
                .IsUnicode(False)

            modelBuilder.Entity(Of Products)() _
                .Property(Function(e) e.productVendor) _
                .IsUnicode(False)

            modelBuilder.Entity(Of SecurityQuestions)() _
                .Property(Function(e) e.question_text) _
                .IsUnicode(False)
        End Sub
    End Class
End Namespace