namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface ISelectable {
        void OnSelected();
        void OnDeselected();
        void OnBecameVisible();
        void OnBecameInvisible();
    }
}