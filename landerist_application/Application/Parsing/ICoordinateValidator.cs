namespace landerist_library.Application.Parsing;

public interface ICoordinateValidator
{
    bool Contains(double latitude, double longitude);
}
