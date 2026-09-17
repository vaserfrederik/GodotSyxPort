using System.Collections.Generic;

namespace View.Tool
{
    public interface ToolConfig
    {
        void AddUI(List<RENDEROBJ> uis)
        {
        }

        void ActivateAction()
        {
        }

        void DeactivateAction()
        {
        }

        bool Back()
        {
            return true;
        }

        void Update(bool UIHovered)
        {
        }
    }
}