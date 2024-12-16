using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenGL_Game.Components;
using OpenTK.Mathematics;
using OpenGL_Game.Managers;

public class ComponentAudio : IComponent
{
    private int audioSource;
    private int audioBuffer;
    private Vector3 position;
    private Vector3 velocity;

    ComponentTypes IComponent.ComponentType => ComponentTypes.COMPONENT_AUDIO;

    public ComponentAudio(string audioFilePath)
    {
        audioBuffer = ResourceManager.LoadAudio(audioFilePath);
        audioSource = AL.GenSource();
        AL.Source(audioSource, ALSourcei.Buffer, audioBuffer);
        AL.Source(audioSource, ALSourceb.Looping, true);

        AL.SourcePlay(audioSource);
    }

    public void SetPosition(Vector3 emitterPosition)
    {
        position = emitterPosition;
        Update();
    }

    public void SetVelocity(Vector3 emitterVelocity)
    {
        velocity = emitterVelocity;
        Update();
    }

    public void Play()
    {
        AL.SourcePlay(audioSource);
    }

    public void Stop()
    {
        AL.SourceStop(audioSource);
    }

    public void CleanUp()
    {
        AL.DeleteSource(audioSource);
        AL.DeleteBuffer(audioBuffer);
    }

    public void Update()
    {
        AL.Source(audioSource, ALSource3f.Position, ref position);
        AL.Source(audioSource, ALSource3f.Velocity, ref velocity);
    }
}