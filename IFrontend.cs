interface IFrontend : IDisposable
{
    string Name { get; }
    IScreen GetScreen();
}
