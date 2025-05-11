using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace GameFramework.Rendering
{
    public static class ShaderHelper
    {
        public static int CreateProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            string vertexShaderSource;
            try
            {
                vertexShaderSource = File.ReadAllText(vertexShaderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading vertex shader file {vertexShaderPath}: {e.Message}");
                throw;
            }

            string fragmentShaderSource;
            try
            {
                fragmentShaderSource = File.ReadAllText(fragmentShaderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading fragment shader file {fragmentShaderPath}: {e.Message}");
                throw;
            }

            int vs = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fs = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vs);
            GL.AttachShader(program, fs);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"Error linking shader program: {infoLog}");
                GL.DeleteProgram(program);
                throw new Exception($"Error linking shader program: {infoLog}");
            }

            // Shaders are linked into the program, they can be detached and deleted
            GL.DetachShader(program, vs);
            GL.DetachShader(program, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            return program;
        }

        private static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus);
            if (compileStatus == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"Error compiling shader of type {type}: {infoLog}");
                GL.DeleteShader(shader);
                throw new Exception($"Error compiling shader of type {type}: {infoLog}");
            }
            return shader;
        }
    }
}
