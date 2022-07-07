using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundAssetManager : AssetManager
{
    public Dictionary<int, AudioClip> sounds = new Dictionary<int, AudioClip>();

    List<WaitingForSound> waitingForSounds = new List<WaitingForSound>();
    List<WaitingForSoundWithTime> waitingForTimedSounds = new List<WaitingForSoundWithTime>();
    public List<int> requestedSounds = new List<int>();

    public override int getAssetID() => 3;
    public override int getPacketID() => 8;

    public override void ProcessData(Manager manager, byte[] data)
    {
        Debug.Log("Processing data for sound with " + data.Length + " size");

        // unpack
        int sound_id = BitConverter.ToInt32(data, 0);
        int channels = BitConverter.ToInt32(data, 4);
        int sample_rate = BitConverter.ToInt32(data, 8);
        int sample_count = BitConverter.ToInt32(data, 12);

        // create audio clip
        AudioClip clip = AudioClip.Create(sound_id.ToString(), sample_count, channels, sample_rate, false);

        // create float array for samples from data
        float[] samples = new float[sample_count];
        for (int i = 0; i < sample_count; i++)
            samples[i] = BitConverter.ToSingle(data, (i * 4) + 16);

        // set audio clip data and save audio clip
        clip.SetData(samples, 0);
        sounds.Add(sound_id, clip);

        Debug.Log("Created sound " + sound_id + " with " + samples.Length + " samples!");

        // play sounds for all those waiting
        lock(waitingForSounds)
        {
            foreach (WaitingForSound waiting in waitingForSounds)
            {
                if (waiting.soundID == sound_id)
                {
                    if (waiting.source != null) waiting.source.PlayOneShot(clip, waiting.volume);
                    waiting.shouldRemove = true;
                }
            }
            waitingForSounds.RemoveAll(shouldRemove);
        }
        lock (waitingForTimedSounds)
        {
            foreach (WaitingForSoundWithTime waiting in waitingForTimedSounds)
            {
                if (waiting.soundID == sound_id)
                {
                    if (waiting.source != null)
                    {
                        float seconds_since_request = Time.realtimeSinceStartup - waiting.startTime;
                        Debug.Log("Time since request " + seconds_since_request);
                        if (seconds_since_request < clip.length)
                        {
                            waiting.source.clip = clip;
                            waiting.source.volume = waiting.volume;
                            waiting.source.time = seconds_since_request;
                            waiting.source.Play();
                            Debug.Log("Source time " + waiting.source.time);
                        }
                    }
                    waiting.shouldRemove = true;
                }
            }
            waitingForSounds.RemoveAll(shouldRemove);
        }

        if (requestedSounds.Contains(sound_id)) requestedSounds.Remove(sound_id);
    }

    private bool shouldRemove(WaitingForSound waiting)
    {
        return waiting.shouldRemove;
    }

    private bool shouldRemove(WaitingForSoundWithTime waiting)
    {
        return waiting.shouldRemove;
    }

    public override void Request(Manager manager, int id)
    {
        if (requestedSounds.Contains(id)) return;
        manager.assetClient.sendPacket(0x07, BitConverter.GetBytes(id));
        requestedSounds.Add(id);
    }

    public void playSoundFromObject(Manager manager, GameObject gameObject, int id, float volume, bool waitForDownload)
    {
        // get audio source
        AudioSource source = gameObject.GetComponent<AudioSource>();

        // make sure game object has audio source object
        if (source == null) source = gameObject.AddComponent<AudioSource>();

        // if we have sound play it now
        if (sounds.ContainsKey(id))
        {
            source.PlayOneShot(sounds[id], volume);
        }
        // otherwise save to the lists and request
        else
        {
            if (!waitForDownload)
            {
                waitingForSounds.Add(new WaitingForSound(source, id, volume));
            }
            else
            {
                waitingForTimedSounds.Add(new WaitingForSoundWithTime(source, id, volume, Time.realtimeSinceStartup));
            }
            Request(manager, id);
        }
    }

    class WaitingForSound
    {
        public AudioSource source;
        public int soundID;
        public float volume;

        public bool shouldRemove = false;

        public WaitingForSound(AudioSource source, int soundID, float volume)
        {
            this.source = source;
            this.soundID = soundID;
            this.volume = volume;
        }
    }

    class WaitingForSoundWithTime
    {
        public AudioSource source;
        public int soundID;
        public float volume;
        public float startTime;

        public bool shouldRemove = false;

        public WaitingForSoundWithTime(AudioSource source, int soundID, float volume, float startTime)
        {
            this.source = source;
            this.soundID = soundID;
            this.volume = volume;
            this.startTime = startTime;
        }
    }
}
