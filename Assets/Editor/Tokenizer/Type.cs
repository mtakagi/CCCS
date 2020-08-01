using System.Collections.Generic;

namespace CCCS
{
    public enum TypeKind
    {
        Void,
        Bool,
        Char,
        Short,
        Int,
        Long,
        Enum,
        Pointer,
        Array,
        Struct,
        Function
    }

    public class Type
    {
        public static readonly Type VoidType;
        public static readonly Type BoolType;
        public static readonly Type CharType;
        public static readonly Type ShortType;
        public static readonly Type IntType;
        public static readonly Type LongType;

        static Type()
        {
            VoidType = new Type(TypeKind.Void, 1, 1);
            BoolType = new Type(TypeKind.Bool, 1, 1);
            CharType = new Type(TypeKind.Char, 1, 1);
            ShortType = new Type(TypeKind.Short, 2, 2);
            IntType = new Type(TypeKind.Int, 4, 4);
            LongType = new Type(TypeKind.Long, 8, 8);
        }


        public TypeKind Kind { get; internal set; }
        public int Size { get; internal set; }
        public int Align { get; internal set; }
        public bool IsIncomplete { get; internal set; }

        public Type BaseType { get; internal set; }
        public int Length { get; private set; }
        public Type ReturnType { get; internal set; }

        public bool IsInteger
        {
            get
            {
                return this.Kind == TypeKind.Bool
                || this.Kind == TypeKind.Char
                || this.Kind == TypeKind.Short
                || this.Kind == TypeKind.Int
                || this.Kind == TypeKind.Long;
            }
        }

        public Type(TypeKind kind, int size, int align)
        {
            this.Kind = kind;
            this.Size = size;
            this.Align = align;
        }

        public static Type PointerTo(Type baseType)
        {
            var type = new Type(TypeKind.Pointer, 8, 8);
            type.BaseType = baseType;

            return type;
        }

        public static Type ArrayOf(Type baseType, int length)
        {
            var type = new Type(TypeKind.Array, baseType.Size * length, baseType.Align);
            type.BaseType = baseType;
            type.Length = length;

            return type;
        }

        public static Type FunctionType(Type returnType)
        {
            var type = new Type(TypeKind.Function, 1, 1);
            type.ReturnType = returnType;

            return type;
        }

        public static Type EnumType() => new Type(TypeKind.Enum, 4, 4);

        public static Type StructType()
        {
            var type = new Type(TypeKind.Struct, 0, 1);
            type.IsIncomplete = true;

            return type;
        }

        public static void AddType(Node node)
        {
            if (node == null)
            {
                return;
            }

            AddType(node.Next);
            AddType(node.Lhs);
            AddType(node.Rhs);
            AddType(node.Cond);
            AddType(node.Then);
            AddType(node.Els);
            AddType(node.Init);
            AddType(node.Inc);

            // for (var n = node.Body; n != null; n = n.Body)
            // {
            //     AddType(n);
            // }
            for (var n = node.Args; n != null; n = n.Args)
            {
                AddType(n);
            }

            switch (node.Kind)
            {
                case NodeKind.Add:
                case NodeKind.Sub:
                case NodeKind.Mul:
                case NodeKind.Div:
                case NodeKind.Equal:
                case NodeKind.NotEqual:
                case NodeKind.LesserThan:
                case NodeKind.LesserThanOrEqual:
                    node.Type = Type.LongType;
                    return;
                case NodeKind.Assign:
                    node.Type = node.Lhs.Type;
                    return;
                case NodeKind.LeftVariable:
                    node.Type = Type.IntType;
                    return;
                // case NodeKind.Body:
                //     AddType(node.Next);
                // return;
                // case ND_TERNARY:
                //     node->ty = node->then->ty;
                //     return;
                // case ND_COMMA:
                //     node->ty = node->rhs->ty;
                //     return;
                // case ND_MEMBER:
                //     node->ty = node->member->ty;
                //     return;
                // case NodeKind.Address:
                //     if (node.Lhs.Type.kind == TypeKind.Array)
                //         node.Type = PointerTo(node.Lhs.Type.BaseType);
                //     else
                //         node.Type = PointerTo(node.Lhs.Type);
                //     return;
                //             case NodeKind.Derefrence:
                //                 {
                //                     if (!node->lhs->ty->base)
                // //   error_tok(node->tok, "invalid pointer dereference");

                //                     Type ty = node.Lhs.Type.BaseType;
                //                     if (Type.Kind == TypeKind.Void)
                //                         // error_tok(node->tok, "dereferencing a void pointer");
                //                         if (ty.kind == TypeKind.Struct && ty.IsIncomplete)
                //                             // error_tok(node->tok, "incomplete struct type");
                //                             node.Type = ty;
                //                     return;
                //                 }
                // case ND_STMT_EXPR:
                //     {
                //         Node* last = node->body;
                //         while (last->next)
                //             last = last->next;
                //         node->ty = last->ty;
                //         return;

                //     }
                default:
                    node.Type = Type.LongType;
                    return;
            }
        }

        public static void AddType(List<Function> func)
        {
            foreach (var f in func)
            {
                for (var n = f.Node; n != null; n = n.Next)
                {
                    AddType(n);
                }
            }
        }

        public static int AlignTo(int n, int align)
        {
            return (n + align - 1) & (~align - 1);
        }
    }
}
