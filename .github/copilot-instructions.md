<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This is a game creation framework project. The core is written in C# with Python scripting capabilities. It supports web, mobile, and desktop platforms. Please ensure generated code aligns with these requirements.

## Framework Overview
- **Core**: The framework is built in C# to ensure high performance and cross-platform compatibility.
- **Scripting**: Python is used for scripting to allow flexibility and ease of use for game developers.
- **Platforms**: The framework supports web, mobile, and desktop game development, ensuring a wide reach for games built on it.

## Best Practices
1. **Modular Design**: Ensure that all components are modular and reusable to facilitate scalability and maintainability.
2. **Cross-Platform Compatibility**: Always test features on all supported platforms (web, mobile, desktop) to ensure consistent behavior.
3. **Performance Optimization**: Optimize code for performance, especially for mobile platforms where resources are limited.
4. **Documentation**: Document all public APIs and provide examples to help developers integrate and use the framework effectively.

## Development Approach
- **Test-Driven Development (TDD)**: Follow TDD principles by writing tests before implementing features. This ensures that the framework is built with reliability and correctness in mind.

### Additional Note:
Always create the test before implementing the code to adhere strictly to TDD principles. Additionally, always run the test suite after making code changes to ensure no regressions are introduced.
Assume that commands are run in powershell unless otherwise specified.

## Testing Guidelines
- **Unit Tests**: Write unit tests for all core functionalities to ensure reliability.
- **Integration Tests**: Test the integration of Python scripts with the C# core to verify seamless interaction.
- **Platform-Specific Tests**: Conduct tests on web, mobile, and desktop platforms to identify and fix platform-specific issues.
- **Automated Testing**: Use automated testing tools to streamline the testing process and catch regressions early.

## Test Suite Setup
1. **Unit Testing Framework**: Use `xUnit` for writing and running unit tests.
2. **Integration Testing**: Set up integration tests to validate the interaction between the C# core and Python scripts.
3. **Platform-Specific Testing**: Configure test environments for web, mobile, and desktop platforms to ensure compatibility.
4. **Automated Testing**: Integrate automated testing tools like `dotnet test` to streamline the testing process.

## Running the Test Suite
To run the test suite and ensure all tests pass:

1. Open a terminal in the project root directory.
2. Execute the following command to run all tests:
   ```
   dotnet test
   ```
3. Review the test results in the terminal output.
4. If any tests fail, investigate the cause by checking the test implementation and the related code.
5. Fix any issues and re-run the tests to confirm the fixes.

By regularly running the test suite, you can ensure the framework remains reliable and free of regressions.

## Debugging
If a definition already exists in the namespace, search each file for the definition and remove duplicates.