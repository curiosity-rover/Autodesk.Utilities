# Autodesk.Utilities

## Overview

The `Autodesk.Utilities` repository provides a collection of utilities, extensions, and custom functions using various Autodesk APIs. These tools are designed to assist in writing extensions and other applications, making it easier to work with Autodesk products like Inventor.

## Features

- **CurveExtensions**: A set of extension methods for `DrawingCurve` objects in Autodesk Inventor. These methods include functionality for calculating similarity between curves, determining curve orientation (vertical/horizontal), and more.
- **Utility Functions**: Helper functions to streamline common tasks in Autodesk applications, such as geometry calculations and object comparisons.
- **Customization**: Easily extend and customize the provided utilities to fit specific project needs.

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/autodesk.utilities.git
   ```
2. **Open the project in Visual Studio or your preferred IDE.**
3. **Build the project to generate the necessary DLLs for use in Autodesk Inventor or other Autodesk applications.**

## Usage

- **Curve Extensions:**
  ```csharp
  using Autodesk.InventorUtils.Extensions;

  // Example: Calculate similarity between two curves
  double similarity = curve1.CalculateSimilarity(curve2);
  ```
- **Additional Utilities:**
  Explore the provided classes and methods to see how they can assist with your specific needs in Autodesk development.

## Contributing

We welcome contributions to enhance the utility library. Please fork the repository and submit a pull request with your improvements or bug fixes.

## License

This project is licensed under the MIT License.

---

Feel free to modify this README description to better suit your specific project details and requirements.
