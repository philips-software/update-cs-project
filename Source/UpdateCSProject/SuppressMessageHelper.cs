// Copyright Koninklijke Philips N.V. 2021

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Helper class for generating Global suppression file
    /// </summary>
    public static class SuppressMessageHelper {

        private static readonly Dictionary<string, string> predefinedClassMap =
            new Dictionary<string, string> {
                { "void", "System.Void" },
                { "object", "System.Object" },
                { "bool", "System.Boolean" },
                { "char", "System.Char" },
                { "byte", "System.Byte" },
                { "sbyte", "System.SByte" },
                { "short", "System.Int16" },
                { "int", "System.Int32" },
                { "long", "System.Int64" },
                { "ushort", "System.UInt16" },
                { "uint", "System.UInt32" },
                { "ulong", "System.UInt64" },
                { "float", "System.Single" },
                { "double", "System.Double" },
                { "string", "System.String" }
            };

        // Some classes that fail to resolve (Seems to be due to missing ASP.NetCore reference)
        private static readonly Dictionary<string, string> knownMissingClasses =
            new Dictionary<string, string> {
                {"ActionExecutingContext", "Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext"},
                {"ActionExecutionDelegate", "Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate"},
                {"ActionResult", "Microsoft.AspNetCore.Mvc.ActionResult"},
                {"AuthorizationFilterContext", "Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext"},
                {"AuthorizationHandlerContext", "Microsoft.AspNetCore.Mvc.Filters.AuthorizationHandlerContext"},
                {"AutomationElement", "System.Windows.Automation.AutomationElement"},
                {"By", "OpenQA.Selenium.By"},
                {"CancellationToken", "System.Threading.CancellationToken"},
                {"Claim", "System.Security.Claims.Claim"},
                {"Dictionary", "System.Collections.Dictionary"},
                {"DoWorkEventArgs", "System.ComponentModel.DoWorkEventArgs"},
                {"FileStream", "System.IO.FileStream"},
                {"Guid", "System.Guid"},
                {"HttpContext", "Microsoft.AspNetCore.Http.HttpContext"},
                {"HttpRequest", "Microsoft.AspNetCore.Http.HttpRequest"},
                {"HttpResponse", "Microsoft.AspNetCore.Http.HttpResponse"},
                {"IActionResult", "Microsoft.AspNetCore.Mvc.IActionResult"},
                {"IApplicationBuilder", "Microsoft.AspNetCore.Builder.IApplicationBuilder"},
                {"ICollection", "System.Collections.ICollection"},
                {"IConfiguration", "Microsoft.Extensions.Configuration.IConfiguration"},
                {"IConfigurationRoot", "Microsoft.Extensions.Configuration.IConfigurationRoot"},
                {"IConfigurationBuilder", "Microsoft.Extensions.Configuration.IConfigurationBuilder"},
                {"IDictionary", "System.Collections.IDictionary"},
                {"IEnumerable", "System.Collections.IEnumerable"},
                {"IMvcCoreBuilder", "Microsoft.Extensions.DependencyInjection.IMvcCoreBuilder"},
                {"ISearchContext", "OpenQA.Selenium.ISearchContext"},
                {"IServiceCollection", "Microsoft.Extensions.DependencyInjection.IServiceCollection"},
                {"IWebDriver", "OpenQA.Selenium.IWebDriver"},
                {"JToken", "Newtonsoft.Json.Linq.JToken"},
                {"List", "System.Collections.List"},
                {"ModelBindingContext", "Microsoft.AspNetCore.Mvc.ModelBindingContext"},
                {"MvcOptions", "Microsoft.AspNetCore.Mvc.MvcOptions"},
                {"OutputFormatterCanWriteContext", "Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterCanWriteContext"},
                {"OutputFormatterWriteContext", "Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext"},
                {"ResourceExecutingContext", "Microsoft.AspNetCore.Mvc.Filters.ResourceExecutingContext"},
                {"Stream", "System.IO.Stream"},
                {"StringBuilder", "System.Text.StringBuilder"},
                {"Task", "System.Threading.Tasks.Task"},
                {"UITestControl", "Microsoft.VisualStudio.TestTools.UITesting.UITestControl"},
                {"Uri", "System.Uir"},
                {"Type", "System.Type"},
                //Syntax node is not within syntax tree
                {"ArrayList", "System.Collections.ArrayList"},
                {"Identifier", "Philips.Platform.Common.Identifier"}
            };

        public static string WriteNamespaceSuppression(string code, NamespaceDeclarationSyntax ns) {
            if (ns == null) {
                throw new ArgumentNullException(nameof(ns));
            }
            // Target Format: ~N:Namespace
            var target = "~N:" + ns.Name;
            // Namespace could be long; maybe more than one line
            if (target.Contains(Environment.NewLine)) {
                target = target.Replace(Environment.NewLine, string.Empty).Trim();
            }
            return WriteAttribute(code, "namespace", target);
        }

        public static string WriteInterfaceSuppression(string code, InterfaceDeclarationSyntax intf) {
            if (intf == null) {
                throw new ArgumentNullException(nameof(intf));
            }
            // Target Format ~T:Namespace.Interface
            var enumName = GetInterfaceName(intf);
            var nsName = GetNamespaceName(intf);
            var target = "~T:" + nsName + "." + enumName;
            return WriteAttribute(code, "type", target);
        }

        public static string WriteEnumSuppression(string code, EnumDeclarationSyntax enm) {
            if (enm == null) {
                throw new ArgumentNullException(nameof(enm));
            }
            // Target Format ~T:Namespace.Enum
            var enumName = GetEnumName(enm);
            var nsName = GetNamespaceName(enm);
            var target = "~T:" + nsName + "." + enumName;
            return WriteAttribute(code, "type", target);
        }

        public static string WriteDelegateSuppression(string code, DelegateDeclarationSyntax dlg) {
            if (dlg == null) {
                throw new ArgumentNullException(nameof(dlg));
            }
            // Target Format ~T:Namespace.Delegate
            var delegateName = GetDelegateName(dlg);
            var nsName = GetNamespaceName(dlg);
            var target = "~T:" + nsName + "." + delegateName;
            return WriteAttribute(code, "type", target);
        }

        public static string WriteClassSuppression(string code, TypeDeclarationSyntax cls) {
            if (cls == null) {
                throw new ArgumentNullException(nameof(cls));
            }
            // Target Format ~T:Namespace.Class
            var className = GetClassName(cls);
            var nsName = GetNamespaceName(cls);
            var target = "~T:" + nsName + "." + className;
            return WriteAttribute(code, "type", target);
        }

        public static string WriteEventSuppression(string code, EventDeclarationSyntax evt) {
            if (evt == null) {
                throw new ArgumentNullException(nameof(evt));
            }
            // Target Format ~E:Namespace.Class.EventName
            var evtName = evt.Identifier.Text;
            var className = GetClassName(evt.Parent);
            var nsName = GetNamespaceName(evt);
            var target = "~E:" + nsName + "." + className + "." + evtName;
            return WriteAttribute(code, "member", target);
        }

        public static string WritePropertySuppression(string code, PropertyDeclarationSyntax prop) {
            if (prop == null) {
                throw new ArgumentNullException(nameof(prop));
            }
            // Target Format: ~P:Namespace.Class.PropertyName
            var propName = prop.Identifier.Text;
            var className = GetClassName(prop.Parent);
            var nsName = GetNamespaceName(prop);
            // TODO: Handle "Item" properties (and their parameters).
            var target = "~P:" + nsName + "." + className + "." + propName;
            return WriteAttribute(code, "member", target);
        }

        public static string WriteMethodSuppression(string code, MethodDeclarationSyntax method, SemanticModel model) {
            if (method == null) {
                throw new ArgumentNullException(nameof(method));
            }
            // Target Format ~M:Namespace.Class.Method(ArgumentTypes)~ReturnType
            var methodName = GetMethodName(method);
            var args = "(" + string.Join(
                ",",
                method.ParameterList.Parameters.Select(p => GetParameterTypeName(p, model))
            ) + ")";
            var className = GetClassName(method.Parent);
            var nsName = GetNamespaceName(method);
            var retName = GetTypeName(method.ReturnType, model);
            if (method.ReturnType is PredefinedTypeSyntax predefinedTypeSyntax &&
                predefinedTypeSyntax.Keyword.Text.Equals("void")) {
                retName = string.Empty;
            }
            var target = "~M:" + nsName + "." + className + "." + methodName + args;
            if (!string.IsNullOrEmpty(retName)) {
                target += "~" + retName;
            }
            return WriteAttribute(code, "member", target);
        }

        public static string WriteFieldSuppression(string code, FieldDeclarationSyntax field) {
            if (field == null) {
                throw new ArgumentNullException(nameof(field));
            }
            // Target Format: ~F:Namespace.Class.Field
            var varName = field.Declaration.Variables.First().Identifier.Text;
            var className = GetClassName(field.Parent);
            var nsName = GetNamespaceName(field);
            var target = "~F:" + nsName + "." + className + "." + varName;
            return WriteAttribute(code, "member", target);
        }

        private static string WriteAttribute(string code, string scope, string target) {
            var builder = new StringBuilder(100);
            builder.Append(
                "[assembly: SuppressMessage(\"");
            builder.Append("Build");
            builder.Append("\", \"");
            builder.Append(code);
            builder.Append("\", Justification = \"Baseline Reference\", Scope = \"");
            builder.Append(scope);
            builder.Append("\", Target = \"");
            builder.Append(target);
            builder.AppendLine("\")]");
            return builder.ToString();
        }

        private static string GetParameterTypeName(ParameterSyntax paramDef, SemanticModel model) {
            var isRefOrOut =
                paramDef.Modifiers.Any(m => m.Kind() == SyntaxKind.OutKeyword || m.Kind() == SyntaxKind.RefKeyword);
            var typeName = GetTypeName(paramDef.Type, model);
            return isRefOrOut ? typeName + "@" : typeName;
        }

        private static string GetNamespaceName(SyntaxNode node) {
            string nsName = "";
            if (node.Ancestors().FirstOrDefault(
                    ancestor => ancestor.IsKind(SyntaxKind.NamespaceDeclaration)
                ) is NamespaceDeclarationSyntax ns) {
                nsName = ns.Name.ToString();
            }

            // Namespace could be long; maybe more than one line
            if (nsName.Contains(Environment.NewLine)) {
                nsName = nsName.Replace(Environment.NewLine, string.Empty).Trim();
            }

            return nsName;
        }

        private static string GetDelegateName(SyntaxNode node) {
            string deletegateName = "";
            int? numGenerics = null;
            // TypeDeclaration can define a Class or a Struct.
            if (node is DelegateDeclarationSyntax delegateDeclarationSyntax) {
                deletegateName = delegateDeclarationSyntax.Identifier.Text;
                numGenerics = delegateDeclarationSyntax.TypeParameterList?.Parameters.Count;
            }
            var fullDelegateName = GetGenericName(deletegateName, numGenerics);
            // Check if we're an inner class
            var parent = node.Parent;
            if (parent != null && parent.IsKind(SyntaxKind.ClassDeclaration)) {
                return GetClassName(node.Parent) + "." + fullDelegateName;
            }

            return fullDelegateName;
        }

        private static string GetInterfaceName(SyntaxNode node) {
            string intfName = "";
            int? numGenerics = null;
            if (node is InterfaceDeclarationSyntax intfDeclarationSyntax) {
                intfName = intfDeclarationSyntax.Identifier.Text;
                numGenerics = 0;
            }
            var fullInterfaceName = GetGenericName(intfName, numGenerics);
            // Check if interface is declared inside a class
            var parent = node.Parent;
            if (parent != null && parent.IsKind(SyntaxKind.ClassDeclaration)) {
                return GetClassName(node.Parent) + "." + fullInterfaceName;
            }

            return fullInterfaceName;
        }

        private static string GetEnumName(SyntaxNode node) {
            string enumName = "";
            int? numGenerics = null;
            if (node is EnumDeclarationSyntax enumDeclarationSyntax) {
                enumName = enumDeclarationSyntax.Identifier.Text;
                numGenerics = 0;
            }
            var fullEnumName = GetGenericName(enumName, numGenerics);
            // Check if enum is declared inside a class
            var parent = node.Parent;
            if (parent != null && parent.IsKind(SyntaxKind.ClassDeclaration)) {
                return GetClassName(node.Parent) + "." + fullEnumName;
            }

            return fullEnumName;
        }

        private static string GetClassName(SyntaxNode node) {
            string className = "";
            int? numGenerics = null;
            // TypeDeclaration can define a Class or a Struct.
            if (node is TypeDeclarationSyntax typeDeclaration) {
                className = typeDeclaration.Identifier.Text;
                numGenerics = typeDeclaration.TypeParameterList?.Parameters.Count;
            }
            var fullClassName = GetGenericName(className, numGenerics);
            // Check if we're an inner class
            var parent = node.Parent;
            if (parent != null && parent.IsKind(SyntaxKind.ClassDeclaration)) {
                return GetClassName(node.Parent) + "." + fullClassName;
            }

            return fullClassName;
        }

        private static string GetTypeName(TypeSyntax typeDef, SemanticModel model) {
            // TODO: function pointer and Ref types.
            string typeName = "UNKNOWN-" + typeDef.GetType().FullName;
            if (typeDef is IdentifierNameSyntax nameSyntax) {
                // Include full namespace of the type.
                typeName = GetTypeNameFromModel(nameSyntax, model);
            } else if (typeDef is PredefinedTypeSyntax preDefinedSyntax) {
                typeName = predefinedClassMap[preDefinedSyntax.Keyword.Text];
            } else if (typeDef is PointerTypeSyntax pointerSyntax) {
                typeName = GetTypeName(pointerSyntax.ElementType, model) + "*";
            } else if (typeDef is GenericNameSyntax genericSyntax) {
                var genericList = genericSyntax.TypeArgumentList.Arguments.Select(a => GetTypeName(a, model));
                var genericTypeName = GetTypeNameFromModel(genericSyntax, model);
                if (genericTypeName == null || genericList.Any(x => x == null)) {
                    return typeName;
                }
                var caretIndex = genericTypeName.IndexOf('<');
                var baseTypeName = (caretIndex < 0) ? genericTypeName : genericTypeName.Substring(0, caretIndex);
                //strange but space not allowed between parameters
                var genericArgs = genericList.Select(x => {
                    return x.Replace('<', '{').Replace('>', '}').Replace(" ", string.Empty);
                }).ToList();
                typeName = baseTypeName + "{" + String.Join(",", genericArgs) + "}";
            } else if (typeDef is NullableTypeSyntax nullableSyntax) {
                typeName = "System.Nullable{" + GetTypeName(nullableSyntax.ElementType, model) + "}";
            } else if (typeDef is ArrayTypeSyntax arraySyntax) {
                var ranks = arraySyntax.RankSpecifiers;
                var sizes = "";
                foreach (var rankSpec in ranks) {
                    var rank = rankSpec.Rank;
                    if (rank == 1) {
                        sizes += "[]";
                    } else {
                        var stars = new string[rank];
                        for (var i = 0; i < rank; i++) {
                            stars[i] = "*";
                        }
                        sizes += "[" + String.Join(",", stars) + "]";
                    }
                }
                typeName = GetTypeName(arraySyntax.ElementType, model) + sizes;
            } else if (typeDef is TupleTypeSyntax tupleSyntax) {
                // Tuples are displayed like: (Type1 name1,Type2 name2)
                var tupleTypes =
                    tupleSyntax.Elements.Select(e => GetTypeName(e.Type, model) + " " + e.Identifier.Text);
                typeName = "(" + String.Join(",", tupleTypes) + ")";
            }
            return typeName;
        }

        private static string GetTypeNameFromModel(SimpleNameSyntax typeDef, SemanticModel model) {
            string typeName = typeDef.Identifier.Text;
            ISymbol symbol = null;
            try {
                symbol = model.GetSymbolInfo(typeDef).Symbol;
            } catch (Exception exception) {
                ConsoleWriter.WriteWarning(
                    string.Format("Can't find full namespace of type: {0}, {1}", typeName, exception.Message));
            }
            finally {
                if (symbol == null) {
                    // Cannot resolve the type, last resort: the known wrongs.
                    if (knownMissingClasses.TryGetValue(typeDef.Identifier.Text, out typeName)) {
                        ConsoleWriter.WriteInfo(string.Format("However got same from lookup: {0}", typeName));
                    }
                } else {
                    typeName = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                    //even after using fully qualified format, for few types name still returned in short format
                    if (predefinedClassMap.ContainsKey(typeName)) {
                        typeName = predefinedClassMap[typeName];
                    }
                }
            }
            return typeName;
        }

        private static string GetMethodName(MethodDeclarationSyntax method) {
            var numGenerics = method.TypeParameterList?.Parameters.Count;
            return GetGenericName(method.Identifier.Text, numGenerics);
        }

        private static string GetGenericName(string name, int? numGenerics) {
            var hasGenerics = numGenerics != null && numGenerics > 0;
            return hasGenerics ? $"{name}`{numGenerics}" : name;
        }
    }
}
