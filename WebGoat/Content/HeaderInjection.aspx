<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="HeaderInjection.aspx.vb" Inherits="OWASP.WebGoat.NET.HeaderInjection" EnableEventValidation="false" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">

<h2>Headers</h2>
<asp:Label ID="lblHeaders" runat="server"></asp:Label>
<br />
<h2>Cookies</h2>
<asp:GridView ID="gvCookies" runat="server" AutoGenerateColumns="true" />
</asp:Content>