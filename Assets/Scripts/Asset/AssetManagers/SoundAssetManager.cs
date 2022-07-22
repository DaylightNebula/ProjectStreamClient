using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SoundAssetManager : AssetManager
{
    public Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();

    List<WaitingForSound> waitingForSounds = new List<WaitingForSound>();
    List<WaitingForSoundWithTime> waitingForTimedSounds = new List<WaitingForSoundWithTime>();
    public List<string> requestedSounds = new List<string>();

    public override int getAssetID() => 3;
    public override int getPacketID() => 8;

    public override void ProcessData(Manager manager, byte[] data)
    {
        // unpack id
        int sound_id_length = BitConverter.ToInt32(data, 0);
        byte[] sound_id_bytes = new byte[sound_id_length];
        Buffer.BlockCopy(data, 4, sound_id_bytes, 0, sound_id_length);
        string sound_id = Encoding.UTF8.GetString(sound_id_bytes);

        // unpack other data
        int byteCounter = 4 + sound_id_length;
        int channels = BitConverter.ToInt32(data, byteCounter);
        int sample_rate = BitConverter.ToInt32(data, byteCounter + 4);
        int sample_count = BitConverter.ToInt32(data, byteCounter + 8);
        byteCounter += 12;

        // create audio clip
        AudioClip clip = AudioClip.Create(sound_id, sample_count, channels, sample_rate, false);

        // create float array for samples from data
        float[] samples = new float[sample_count];
        for (int i = 0; i < sample_count; i++)
            samples[i] = BitConverter.ToSingle(data, (i * 4) + byteCounter);

        // set audio clip data and save audio clip
        clip.SetData(samples, 0);
        sounds.Add(sound_id, clip);

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
                        if (seconds_since_request < clip.length)
                        {
                            waiting.source.clip = clip;
                            waiting.source.volume = waiting.volume;
                            waiting.source.time = seconds_since_request;
                            waiting.source.Play();
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

    public override void Request(Manager manager, string id)
    {
        // if we have the requested sound, return
        if (requestedSounds.Contains(id)) return;

        // build request packet
        byte[] idBytes = BitConverter.GetBytes(id.Length);
        byte[] idStringBytes = Encoding.UTF8.GetBytes(id);
        byte[] packet = new byte[4 + id.Length];
        Buffer.BlockCopy(idBytes, 0, packet, 0, 4);
        Buffer.BlockCopy(idStringBytes, 0, packet, 4, idStringBytes.Length);

        // call request packet
        manager.assetClient.sendPacket(
            0x07, 
            packet
        );

        // add id to requested sounds
        requestedSounds.Add(id);
    }

    public void playSoundFromObject(Manager manager, GameObject gameObject, string id, float volume, bool waitForDownload)
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

    public void playSoundAtPosition(Manager manager, Vector3 position, string id, float volume)
    {
        // if we have sound play it now
        if (sounds.ContainsKey(id))
        {
            AudioSource.PlayClipAtPoint(sounds[id], position, volume);
        }
        // otherwise request
        else
            Request(manager, id);
    }

    class WaitingForSound
    {
        public AudioSource source;
        public string soundID;
        public float volume;

        public bool shouldRemove = false;

        public WaitingForSound(AudioSource source, string soundID, float volume)
        {
            this.source = source;
            this.soundID = soundID;
            this.volume = volume;
        }
    }

    class WaitingForSoundWithTime
    {
        public AudioSource source;
        public string soundID;
        public float volume;
        public float startTime;

        public bool shouldRemove = false;

        public WaitingForSoundWithTime(AudioSource source, string soundID, float volume, float startTime)
        {
            this.source = source;
            this.soundID = soundID;
            this.volume = volume;
            this.startTime = startTime;
        }
    }
}
