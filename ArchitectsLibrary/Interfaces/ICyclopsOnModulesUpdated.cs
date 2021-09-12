namespace ArchitectsLibrary.Interfaces
{
    /// <summary>
    /// Used for when the modules in a cyclops are changed.
    /// </summary>
    public interface ICyclopsOnModulesUpdated
    {
        /// <summary>
        /// Called whenever any module is equipped into a cyclops.
        /// </summary>
        /// <param name="cyclops">An instance of the Cyclops's <see cref="SubRoot"/>.</param>
        /// <param name="modulesCount">How many of this module is equipped in the cyclops.</param>
        void OnModuleCountChanged(SubRoot cyclops, int modulesCount);
    }
}