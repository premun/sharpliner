using System;
using System.Collections.Generic;

namespace Sharpliner.GitHubActions;

public record DockerOptions { }

public record ContainerCredentials
{
    public string? Username { get; set; }

    public string? Password { get; set; }
}

/// <summary>
/// Represents the docker container that will be used to run the job.
/// </summary>
public record Container
{
    /// <summary>
    /// Use the container with the given ID will be used to run the job.
    /// </summary>
    /// <param name="image">Id of the image to use.</param>
    /// <exception cref="ArgumentNullException">Raised if the image Id is null or empty.</exception>
    public Container(string image)
    {
        if (string.IsNullOrEmpty(image))
            throw new ArgumentNullException(nameof(image));
        Image = image;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="image"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public Container(string image, string? username, string? password) : this(image)
    {
        Credentials = new() { Username = username, Password = password };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="image"></param>
    /// <param name="credentials"></param>
    public Container(string image, ContainerCredentials credentials)
        : this(image, credentials.Username, credentials.Password)
    {
    }

    public string Image { get; }

    /// <summary>
    /// Get/Set the credentials of the image.
    /// </summary>
    public ContainerCredentials? Credentials { get; set; }

    /// <summary>
    /// Get/Set the environment variables preset in the image.
    /// </summary>
    public Dictionary<string, string> Env { get; } = new();

    /// <summary>
    /// Get/Set the list of available ports in the image.
    /// </summary>
    public List<int> Ports { get; set; } = new();

    /// <summary>
    /// Get/Set the array of volumes to be used in the image.
    /// </summary>
    public List<string> Volumes { get; set; } = new();

    /// <summary>
    /// Get/Set additional create options for the docker image.
    /// </summary>
    public List<DockerOptions> DockerOptions { get; set; } = new();

    public static implicit operator Container(string image) => new(image);
}
