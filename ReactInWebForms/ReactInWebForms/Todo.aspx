<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Todo.aspx.cs" Inherits="ReactInWebForms.Todo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="Todo.scss" />
</head>
<body>
    <form id="form1" runat="server" ClientIdMode="Static">
    </form>

    <div id="divReactContainer"></div>
    <script type="text/javascript" src="React/Todo.jsx"></script>
</body>
</html>
