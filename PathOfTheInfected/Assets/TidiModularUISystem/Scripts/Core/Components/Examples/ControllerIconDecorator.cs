using UnityEngine;
using UnityEngine.UIElements;

namespace TidiModularUISystem.Core.Examples
{
    [UxmlElement]
    public partial class ControllerIconDecorator : Image
    {
        public ControllerIconDecorator()
        {
            AddToClassList("controller__icon");
        }
    }
}