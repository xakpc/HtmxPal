<!-- Improved compatibility of back to top link: See: https://github.com/othneildrew/Best-README-Template/pull/73 -->
<a name="readme-top"></a>

<div align="center">
  
<!-- [![GitHub Actions Workflow Status][actions-shield]][actions-url]-->
[![Stargazers][stars-shield]][stars-url]
[![MIT License][license-shield]][license-url]
[![VS Marketplace Downloads][vsm-shield]][vsm-url]
[![Rate it][vsmrating-shield]][vsmrating-url]

</div>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/xakpc/HtmxPal/">
    <img src="resources/htmx-icon.png" alt="Logo" width="150" height="150">
  </a>

<h3 align="center">htmx-pal</h3>

  <p align="center">
    htmx Code Completion and IntelliSense extension for Visual Studio 2019 and 2022
    <br />
    <a href="#getting-started"><strong>Start here »</strong></a>
    <br />
    <br />
    <a href="https://github.com/xakpc/HtmxPal/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    ·
    <a href="https://github.com/xakpc/HtmxPal/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
    ·
    <a href="https://xakpc.info/extending-visual-studio-2022">Read Article</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->
## About The Project

![htmx-pal Extension Screenshot](https://github.com/xakpc/HtmxPal/blob/master/resources/screenshot.PNG)

htmx-pal is a non-official Visual Studio extension that enhances your development experience when working with htmx in Visual Studio 2022. 
It provides code completion and quick info (IntelliSense) features for htmx attributes, making writing and understanding htmx-powered web applications easier.

### Key Features

#### **Code Completion:** Get suggestions for htmx attributes as you type.

![htmx-pal Extension Screenshot](https://github.com/xakpc/HtmxPal/blob/master/resources/code-completion-2.gif)

#### **Attribute Code Completion:** Get possible values for some attributes with .

![htmx-pal Extension Screenshot](https://github.com/xakpc/HtmxPal/blob/master/resources/code-completion-1.gif)

#### **Quick Info:** Hover over htmx attributes to see detailed information and usage tips.

![htmx-pal Extension Screenshot](https://github.com/xakpc/HtmxPal/blob/master/resources/intelli-sense-1.gif)

This extension is designed to boost your productivity when working with HTMX, providing instant access to attribute information and reducing the need to consult external documentation.

### Built With

- [Visual Studio Extensibility (VSIX)](https://docs.microsoft.com/en-us/visualstudio/extensibility/starting-to-develop-visual-studio-extensions?view=vs-2022)
- [C#](https://docs.microsoft.com/en-us/dotnet/csharp/)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

To get htmx-pal up and running in your Visual Studio environment, follow these simple steps.

### Prerequisites

- Visual Studio 2022

### Installation

1. Open Visual Studio
2. Go to Extensions > Manage Extensions
3. Search for "htmx-pal"
4. Click "Download"
5. Restart Visual Studio to complete the installation

Alternatively, you can download the VSIX file directly from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=xakpc.htmx-pal).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage

Once installed, htmx-pal will automatically provide IntelliSense features for htmx attributes in your HTML and Razor files. 
Here's how you can use it:

1. Open an HTML or Razor file in Visual Studio.
2. Start typing an htmx attribute (e.g., `hx-`).
3. The extension will offer code completion suggestions.
4. Select an attribute from the list or continue typing.
5. Hover over htmx attributes to see quick info about their usage and purpose.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->
## Contact

Pavel - [@xakpc](https://twitter.com/xakpc)

Project Link: [https://github.com/xakpc/htmx-pal](https://github.com/xakpc/HtmxPal)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [htmx.org](https://htmx.org/) - high-power tools for html
* [Visual Studio Extensibility Documentation](https://docs.microsoft.com/en-us/visualstudio/extensibility)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->
[stars-shield]: https://img.shields.io/github/stars/xakpc/HtmxPal.svg?style=for-the-badge
[stars-url]: https://github.com/xakpc/HtmxPal/stargazers
[license-shield]: https://img.shields.io/github/license/xakpc/HtmxPal?style=for-the-badge
[license-url]: https://github.com/xakpc/HtmxPal/blob/master/LICENSE.txt
[vsm-shield]: https://img.shields.io/visual-studio-marketplace/i/xakpc.htmx-pal?style=for-the-badge&logo=visualstudio
[vsm-url]: https://marketplace.visualstudio.com/items?itemName=xakpc.htmx-pal
[vsmrating-shield]: https://img.shields.io/visual-studio-marketplace/stars/xakpc.htmx-pal?style=for-the-badge&logo=visualstudio
[vsmrating-url]:https://marketplace.visualstudio.com/items?itemName=xakpc.htmx-pal&ssr=false#review-details
[actions-shield]: https://img.shields.io/github/actions/workflow/status/xakpc/HtmxPal/build.yml?style=for-the-badge&logo=github
[actions-url]: https://github.com/xakpc/HtmxPal/actions
