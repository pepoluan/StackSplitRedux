using System;

namespace StackSplitRedux
    {
    interface IStackSplitAPI
        {
        bool TryRegisterMenu(Type menuType);
        }
    }
