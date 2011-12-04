
namespace MParse.OutputGenerators
{
    using System;
    using System.Collections.Generic;


    public abstract class _Terminal
    {

        private string _text;

        protected _Terminal(string text)
        {
            this._text = text;
        }

        public string Text
        {
            get
            {
                return this._text;
            }
        }

        public abstract ActionResult Action(Stack<int> states, Stack<object> parseStack);

        public override string ToString()
        {
            return this._text;
        }
    }

    public enum ActionResult
    {

        Error,

        Accept,

        ShiftContinue,

        ReduceContinue,
    }

    public interface IReducer
    {

        void Goto(System.Collections.Generic.Stack<int> states);
    }

    public abstract class S : IReducer
    {

        public void Goto(System.Collections.Generic.Stack<int> states)
        {
            int currentState = states.Peek();
            throw new System.Exception("Unexpected GOTO operation.");
        }
    }

    public class _S_E_EOF : S
    {

        private E _t1;

        private EOF _t2;

        public _S_E_EOF(E t1, EOF t2)
        {
            this._t1 = t1;
            this._t2 = t2;
        }

        public E t1
        {
            get
            {
                return this._t1;
            }
        }

        public EOF t2
        {
            get
            {
                return this._t2;
            }
        }

        public static _S_E_EOF ReduceBy(Stack<object> parseTree)
        {
            object o0 = parseTree.Pop();
            object o1 = parseTree.Pop();
            return new _S_E_EOF(((E)(o1)), ((EOF)(o0)));
        }
    }

    public abstract class E : IReducer
    {

        public void Goto(System.Collections.Generic.Stack<int> states)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                states.Push(1);
                return;
            }
            if ((currentState == 5))
            {
                states.Push(9);
                return;
            }
            throw new System.Exception("Unexpected GOTO operation.");
        }
    }

    public class _E_E_Plus_T : E
    {

        private E _t1;

        private Plus _t2;

        private T _t3;

        public _E_E_Plus_T(E t1, Plus t2, T t3)
        {
            this._t1 = t1;
            this._t2 = t2;
            this._t3 = t3;
        }

        public E t1
        {
            get
            {
                return this._t1;
            }
        }

        public Plus t2
        {
            get
            {
                return this._t2;
            }
        }

        public T t3
        {
            get
            {
                return this._t3;
            }
        }

        public static _E_E_Plus_T ReduceBy(Stack<object> parseTree)
        {
            object o0 = parseTree.Pop();
            object o1 = parseTree.Pop();
            object o2 = parseTree.Pop();
            return new _E_E_Plus_T(((E)(o2)), ((Plus)(o1)), ((T)(o0)));
        }
    }

    public class _E_T : E
    {

        private T _t1;

        public _E_T(T t1)
        {
            this._t1 = t1;
        }

        public T t1
        {
            get
            {
                return this._t1;
            }
        }

        public static _E_T ReduceBy(Stack<object> parseTree)
        {
            object o0 = parseTree.Pop();
            return new _E_T(((T)(o0)));
        }
    }

    public abstract class T : IReducer
    {

        public void Goto(System.Collections.Generic.Stack<int> states)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                states.Push(2);
                return;
            }
            if ((currentState == 5))
            {
                states.Push(2);
                return;
            }
            if ((currentState == 6))
            {
                states.Push(10);
                return;
            }
            throw new System.Exception("Unexpected GOTO operation.");
        }
    }

    public class _T_T_Times_F : T
    {

        private T _t1;

        private Times _t2;

        private F _t3;

        public _T_T_Times_F(T t1, Times t2, F t3)
        {
            this._t1 = t1;
            this._t2 = t2;
            this._t3 = t3;
        }

        public T t1
        {
            get
            {
                return this._t1;
            }
        }

        public Times t2
        {
            get
            {
                return this._t2;
            }
        }

        public F t3
        {
            get
            {
                return this._t3;
            }
        }

        public static _T_T_Times_F ReduceBy(Stack<object> parseTree)
        {
            object o0 = parseTree.Pop();
            object o1 = parseTree.Pop();
            object o2 = parseTree.Pop();
            return new _T_T_Times_F(((T)(o2)), ((Times)(o1)), ((F)(o0)));
        }
    }

    public class _T_F : T
    {

        private F _t1;

        public _T_F(F t1)
        {
            this._t1 = t1;
        }

        public F t1
        {
            get
            {
                return this._t1;
            }
        }

        public static _T_F ReduceBy(Stack<object> parseTree)
        {
            object o0 = parseTree.Pop();
            return new _T_F(((F)(o0)));
        }
    }

    public abstract class F : IReducer
    {

        public void Goto(System.Collections.Generic.Stack<int> states)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                states.Push(3);
                return;
            }
            if ((currentState == 5))
            {
                states.Push(3);
                return;
            }
            if ((currentState == 6))
            {
                states.Push(3);
                return;
            }
            if ((currentState == 8))
            {
                states.Push(11);
                return;
            }
            throw new System.Exception("Unexpected GOTO operation.");
        }
    }

    public class _F_Lparen_E_Rparen : F
    {

        private Lparen _t1;

        private E _t2;

        private Rparen _t3;

        public _F_Lparen_E_Rparen(Lparen t1, E t2, Rparen t3)
        {
            this._t1 = t1;
            this._t2 = t2;
            this._t3 = t3;
        }

        public Lparen t1
        {
            get
            {
                return this._t1;
            }
        }

        public E t2
        {
            get
            {
                return this._t2;
            }
        }

        public Rparen t3
        {
            get
            {
                return this._t3;
            }
        }

        public static _F_Lparen_E_Rparen ReduceBy(Stack<object> parseTree)
        {
            object o0 = parseTree.Pop();
            object o1 = parseTree.Pop();
            object o2 = parseTree.Pop();
            return new _F_Lparen_E_Rparen(((Lparen)(o2)), ((E)(o1)), ((Rparen)(o0)));
        }
    }

    public class _F_Id : F
    {

        private Id _t1;

        public _F_Id(Id t1)
        {
            this._t1 = t1;
        }

        public Id t1
        {
            get
            {
                return this._t1;
            }
        }

        public static _F_Id ReduceBy(Stack<object> parseTree)
        {
            object o0 = parseTree.Pop();
            return new _F_Id(((Id)(o0)));
        }
    }

    public class Plus : _Terminal
    {

        public Plus(string text) :
            base(text)
        {
        }

        public override ActionResult Action(Stack<int> states, Stack<object> parseStack)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                return ActionResult.Error;
            }
            if ((currentState == 1))
            {
                states.Push(6);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 2))
            {
                parseStack.Push(_E_T.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 3))
            {
                parseStack.Push(_T_F.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 4))
            {
                parseStack.Push(_F_Id.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 5))
            {
                return ActionResult.Error;
            }
            if ((currentState == 6))
            {
                return ActionResult.Error;
            }
            if ((currentState == 7))
            {
                return ActionResult.Error;
            }
            if ((currentState == 8))
            {
                return ActionResult.Error;
            }
            if ((currentState == 9))
            {
                states.Push(6);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 10))
            {
                parseStack.Push(_E_E_Plus_T.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 11))
            {
                parseStack.Push(_T_T_Times_F.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 12))
            {
                parseStack.Push(_F_Lparen_E_Rparen.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public class Times : _Terminal
    {

        public Times(string text) :
            base(text)
        {
        }

        public override ActionResult Action(Stack<int> states, Stack<object> parseStack)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                return ActionResult.Error;
            }
            if ((currentState == 1))
            {
                return ActionResult.Error;
            }
            if ((currentState == 2))
            {
                states.Push(8);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 3))
            {
                parseStack.Push(_T_F.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 4))
            {
                parseStack.Push(_F_Id.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 5))
            {
                return ActionResult.Error;
            }
            if ((currentState == 6))
            {
                return ActionResult.Error;
            }
            if ((currentState == 7))
            {
                return ActionResult.Error;
            }
            if ((currentState == 8))
            {
                return ActionResult.Error;
            }
            if ((currentState == 9))
            {
                return ActionResult.Error;
            }
            if ((currentState == 10))
            {
                states.Push(8);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 11))
            {
                parseStack.Push(_T_T_Times_F.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 12))
            {
                parseStack.Push(_F_Lparen_E_Rparen.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public class Id : _Terminal
    {

        public Id(string text) :
            base(text)
        {
        }

        public override ActionResult Action(Stack<int> states, Stack<object> parseStack)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                states.Push(4);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 1))
            {
                return ActionResult.Error;
            }
            if ((currentState == 2))
            {
                return ActionResult.Error;
            }
            if ((currentState == 3))
            {
                return ActionResult.Error;
            }
            if ((currentState == 4))
            {
                return ActionResult.Error;
            }
            if ((currentState == 5))
            {
                states.Push(4);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 6))
            {
                states.Push(4);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 7))
            {
                return ActionResult.Error;
            }
            if ((currentState == 8))
            {
                states.Push(4);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 9))
            {
                return ActionResult.Error;
            }
            if ((currentState == 10))
            {
                return ActionResult.Error;
            }
            if ((currentState == 11))
            {
                return ActionResult.Error;
            }
            if ((currentState == 12))
            {
                return ActionResult.Error;
            }
            return ActionResult.Error;
        }
    }

    public class Lparen : _Terminal
    {

        public Lparen(string text) :
            base(text)
        {
        }

        public override ActionResult Action(Stack<int> states, Stack<object> parseStack)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                states.Push(5);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 1))
            {
                return ActionResult.Error;
            }
            if ((currentState == 2))
            {
                return ActionResult.Error;
            }
            if ((currentState == 3))
            {
                return ActionResult.Error;
            }
            if ((currentState == 4))
            {
                return ActionResult.Error;
            }
            if ((currentState == 5))
            {
                states.Push(5);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 6))
            {
                states.Push(5);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 7))
            {
                return ActionResult.Error;
            }
            if ((currentState == 8))
            {
                states.Push(5);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 9))
            {
                return ActionResult.Error;
            }
            if ((currentState == 10))
            {
                return ActionResult.Error;
            }
            if ((currentState == 11))
            {
                return ActionResult.Error;
            }
            if ((currentState == 12))
            {
                return ActionResult.Error;
            }
            return ActionResult.Error;
        }
    }

    public class Rparen : _Terminal
    {

        public Rparen(string text) :
            base(text)
        {
        }

        public override ActionResult Action(Stack<int> states, Stack<object> parseStack)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                return ActionResult.Error;
            }
            if ((currentState == 1))
            {
                return ActionResult.Error;
            }
            if ((currentState == 2))
            {
                parseStack.Push(_E_T.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 3))
            {
                parseStack.Push(_T_F.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 4))
            {
                parseStack.Push(_F_Id.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 5))
            {
                return ActionResult.Error;
            }
            if ((currentState == 6))
            {
                return ActionResult.Error;
            }
            if ((currentState == 7))
            {
                return ActionResult.Error;
            }
            if ((currentState == 8))
            {
                return ActionResult.Error;
            }
            if ((currentState == 9))
            {
                states.Push(12);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if ((currentState == 10))
            {
                parseStack.Push(_E_E_Plus_T.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 11))
            {
                parseStack.Push(_T_T_Times_F.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 12))
            {
                parseStack.Push(_F_Lparen_E_Rparen.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public class EOF : _Terminal
    {

        public EOF(string text) :
            base(text)
        {
        }

        public override ActionResult Action(Stack<int> states, Stack<object> parseStack)
        {
            int currentState = states.Peek();
            if ((currentState == 0))
            {
                return ActionResult.Error;
            }
            if ((currentState == 1))
            {
                return ActionResult.Accept;
            }
            if ((currentState == 2))
            {
                parseStack.Push(_E_T.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 3))
            {
                parseStack.Push(_T_F.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 4))
            {
                parseStack.Push(_F_Id.ReduceBy(parseStack));
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 5))
            {
                return ActionResult.Error;
            }
            if ((currentState == 6))
            {
                return ActionResult.Error;
            }
            if ((currentState == 7))
            {
                return ActionResult.Error;
            }
            if ((currentState == 8))
            {
                return ActionResult.Error;
            }
            if ((currentState == 9))
            {
                return ActionResult.Error;
            }
            if ((currentState == 10))
            {
                parseStack.Push(_E_E_Plus_T.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 11))
            {
                parseStack.Push(_T_T_Times_F.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            if ((currentState == 12))
            {
                parseStack.Push(_F_Lparen_E_Rparen.ReduceBy(parseStack));
                states.Pop();
                states.Pop();
                states.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public class Parser
    {

        public S Parse(IList<_Terminal> tokens)
        {
            System.Collections.Generic.Stack<int> stateStack = new System.Collections.Generic.Stack<int>();
            System.Collections.Generic.Stack<object> parseStack = new System.Collections.Generic.Stack<object>();
            stateStack.Push(0);
            int tokenPos = 0;
            for (
            ; true;
            )
            {
                
            startLoop:
                _Terminal t = tokens[tokenPos];
            startLoop2:
                ActionResult result = t.Action(stateStack, parseStack);
                if ((result == ActionResult.Accept))
                {
                    goto breakLoop;
                }
                if ((result == ActionResult.ShiftContinue))
                {
                    Console.WriteLine("Shift");
                    Console.WriteLine(stateStack.Peek());
                    tokenPos = (tokenPos + 1);
                    if (tokenPos >= tokens.Count)
                        goto startLoop2;
                    goto startLoop;
                }
                if ((result == ActionResult.ReduceContinue))
                {
                    Console.WriteLine("Reduce");
                    Console.WriteLine(stateStack.Peek());
                    IReducer reducer = ((IReducer)(parseStack.Peek()));
                    reducer.Goto(stateStack);
                    Console.WriteLine("Goto");
                    Console.WriteLine(stateStack.Peek());
                    goto startLoop;
                }
                if ((result == ActionResult.Error))
                {
                    throw new System.Exception("Parsing Failed.");
                }
            }
        breakLoop:
            return ((S)(parseStack.Pop()));
        }
    }
}
