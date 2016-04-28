package com.example.byunghwl.plugintest;


import android.annotation.TargetApi;
import android.app.Activity;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.content.Context;
import android.content.Intent;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioRecord;
import android.media.AudioTrack;
import android.media.MediaRecorder;
import android.os.Environment;
import android.os.StatFs;
import android.provider.MediaStore;
import android.util.Log;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;
import java.lang.Math;

import com.google.unity.GoogleUnityActivity;
import com.dropbox.client2.android.AndroidAuthSession;
import com.dropbox.client2.session.AppKeyPair;
import com.dropbox.client2.DropboxAPI;


import com.studio272.googledriveplugin.GoogleDrivePluginActivity;

//class for VAD Record
class VoiceActivityRecord {
    int time, type;

    VoiceActivityRecord(int time, int type)
    {
        this.time = time;
        this.type = type;
    }

    public int getTime() { return this.time;}
    public int getType() { return this.type;} //0 = SILENCE, 1 = SPEAKING, 2 = SHOUTING
}


public class MainActivity extends com.google.unity.GoogleUnityActivity  {

    private int FREQUENCY = 44100;


    //Memory Check function 4/26/2016
    @TargetApi(18)  public static long GetAvailableMemory(String path ){
        long availableSizeInBytes = new StatFs(path).getAvailableBytes();
        Log.i("bhMemoryLog", "Availabe Memory at the given path :" + availableSizeInBytes );
        return availableSizeInBytes;
    }


    //Google Drive Activity Added 3/24/2016
    //-------------------------------------------------------------------------
    public static void launchGoogleDriveActivity() {
        Intent intent = new Intent(currContext, GoogleDrivePluginActivity.class);
        currContext.startActivity(intent);
    }

    private DropboxAPI<AndroidAuthSession> mDBApi;
    static private Context currContext;
    final static private String APP_KEY = "rnj3c5emjhj6qzs";
    final static private String APP_SECRET = "1wd56pmx1hl8173";
    private String accessToken = "";
    private boolean bIsDropboxApiInitated = false;
    private boolean bIsUpdating = false;
    private boolean bIsVRrecord = false;

    public void start_Dropbox_Authentication(){

        this.runOnUiThread(new Runnable(){
            @Override
            public void run() {
                AppKeyPair appKeys = new AppKeyPair(APP_KEY, APP_SECRET);
                AndroidAuthSession session = new AndroidAuthSession(appKeys);
                mDBApi = new DropboxAPI<AndroidAuthSession>(session);
                mDBApi.getSession().startOAuth2Authentication(currContext);
                bIsDropboxApiInitated = true;
                bIsUpdating = true;
            }
        });
    }

    public String getTokenFromNative(){

        String accessToken = "null";

        if(bIsUpdating || !bIsDropboxApiInitated)
            return accessToken;


        if (mDBApi.getSession().authenticationSuccessful()) {
            try {
                // Required to complete auth, sets the access token on the session
                mDBApi.getSession().finishAuthentication();

                accessToken = mDBApi.getSession().getOAuth2AccessToken();

            } catch (IllegalStateException e) {
                Log.i("DbAuthLog", "Error authenticating", e);
                accessToken = "Authfailed";
            }

            ResetDropbox();
        }else{
            accessToken = "Authfailed";
            ResetDropbox();
        }
		Log.i("DbAuthLog", "AuthResult:" + accessToken);
        Log.i("DbAuthLog", "AuthResult_update:" + bIsUpdating);
        Log.i("DbAuthLog", "AuthResult_initiate:" + bIsDropboxApiInitated);

        return accessToken;
    }

    private void ResetDropbox(){
        bIsDropboxApiInitated = false;
        mDBApi =null;
        bIsUpdating = false;
    }

    /*Microphon Checking*/
    public boolean checkHeadsetPlugged(){
        AudioManager am1 = (AudioManager)getSystemService(Context.AUDIO_SERVICE);
        boolean res = false;
        if(am1.isWiredHeadsetOn()){
            res = true;
        }

        return res;
    }
    /*
        Consider Super class order

     */

    @Override
    protected void onCreate(android.os.Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        currContext = this;


    }

    @Override
    protected void onResume() {
        super.onResume();

        if(bIsVRrecord){
            if (isKilled) {
                isKilled = false;
                initialize_recordNplayback(filepath);
                setReverbStrength(this.reverbStrength);
            }
        }

        if(bIsDropboxApiInitated){

            if(mDBApi == null) {
                Log.i("DbAuthLog", "mDBApi NULL ERROR");
                return;
            }

            if (mDBApi.getSession().authenticationSuccessful()) {
                try {
                    // Required to complete auth, sets the access token on the session
                    mDBApi.getSession().finishAuthentication();

                    accessToken = mDBApi.getSession().getOAuth2AccessToken();

                } catch (IllegalStateException e) {
                    Log.i("DbAuthLog", "Error authenticating", e);
                }

            }
            bIsUpdating = false;
        }
    }


    @Override
    protected void onPause() { /* compiled code */
        if(bIsVRrecord){
            isRecording = false;
            isKilled = true;

            try {
                recordingThread.join();
            }
            catch (Exception e) {
                e.printStackTrace();
            }

            record.stop();
            track.stop();
            record.release();
            track.release();

            record=null;
            track=null;

            try {outputStream.close();}
            catch (IOException e) {e.printStackTrace();}
        }

        super.onPause();
    }


    @Override
    protected void onDestroy() { /* compiled code */
        //free the memory here

        isRecording = false;
        isKilled = true;

        try {
            recordingThread.join();
        }
        catch (Exception e) {
            e.printStackTrace();
        }

        record.stop();
        track.stop();
        record.release();
        track.release();

        super.onDestroy();
    }

    //RT playback amplifier volume control
    //use @TargetApi(21) because AudioTrack.setVolume is a higher version (v21) call, need to specify.
    //DON'T CALL THIS BEFORE initialize() BECAUSE THIS CALLS Track WHICH IS INITIALIZED IN initialize()
    @TargetApi(21) public void setReverbStrength(float multiplier) {
        if (multiplier<0f)
            this.reverbStrength = 0f;
        else if (multiplier>1f)
            this.reverbStrength = 1f;
        else
            this.reverbStrength = multiplier;

        track.setVolume(this.reverbStrength);

        Log.i("MainActivity", "Set the reverb strength multiplier to " + this.reverbStrength);
    }
    //------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------

    //threshold detection
    private int latestAverage = 0;
    private int volumeTestSum = 0, volumeTestSampleCount = 0;
    public void startTestThreshold () {
        am = (AudioManager) this.getSystemService(Context.AUDIO_SERVICE);
        am.setMode(AudioManager.STREAM_MUSIC);
        //am.setSpeakerphoneOn(false);
        isKilled = false;

        Log.i("MainActivity", "Start volume testing");
        volumeTestSampleCount = 0;
        volumeTestSum = 0;

        this.setVolumeControlStream(AudioManager.STREAM_MUSIC);

        (recordingThread = new Thread()
        {
            @Override
            public void run()
            {
                volumeTest();
            }
        }).start();

        initRecordAndTrack();
        startRecordAndPlay();
    }

    public int getNowAvg() {
        return latestAverage;
    }

    public int stopTestThreshold () {
        Log.i("MainActivity", "Stop volume testing");

        isRecording = false;
        isKilled = true;

        try {
            recordingThread.join();
        }
        catch (Exception e) {
            e.printStackTrace();
        }

        record.stop();
        track.stop();
        record.release();
        track.release();

        if (volumeTestSampleCount == 0)
            return 0;
        else
            return volumeTestSum / volumeTestSampleCount;
    }

    private void volumeTest() //this is on second thread
    {
        int bufferSizeInBytes = AudioRecord.getMinBufferSize(FREQUENCY, AudioFormat.CHANNEL_IN_MONO, AudioFormat.ENCODING_PCM_16BIT);

        byte byteData[] = new byte[bufferSizeInBytes];
        am.setMode(AudioManager.MODE_IN_COMMUNICATION);
        while (true)
        {
            if (isKilled)
                break;

            if (isRecording) {
                int bufferReadByte = record.read(byteData, 0, bufferSizeInBytes);

                //write to playback track
//                track.write(byteData, 0, bufferReadByte);

                //byte > short array for VAD
                int bufferReadShort = bufferReadByte / 2;
                short[] shortArray = new short[bufferReadShort + 1];
                if (bufferReadByte == 0)
                    continue;

                int j = 0;
                for (j = 0; j < bufferReadByte; j += 2) {
                    ByteBuffer bb = ByteBuffer.allocate(2);
                    bb.order(ByteOrder.LITTLE_ENDIAN);
                    bb.put(byteData[j]);
                    bb.put(byteData[j + 1]);
                    short shortVal = bb.getShort(0);
                    shortArray[j / 2] = shortVal;
                }
                shortArray[j / 2] = '\0';

                int nowSum = volumeTestSum, nowCount = volumeTestSampleCount;
                for (int i = 0; i < bufferReadShort; i++) {
                    volumeTestSum += Math.abs((int) shortArray[i]);
                    volumeTestSampleCount += 1;
                }
                if ((volumeTestSampleCount - nowCount) == 0)
                    latestAverage = 0;
                else
                    latestAverage = (volumeTestSum-nowSum) / (volumeTestSampleCount - nowCount);
            }
        }
    }

    //record variables
    String filepath = "";
    FileOutputStream outputStream = null;
    FileInputStream inputStream = null;

    //voice activity detection (=VAD) variables
    private ArrayList<VoiceActivityRecord> VADRecord;
    private long lastnanosec = 0;
    private int tOpposite, tCurrent, sCurrent = 0; //t = time, s = status: 0 = SILENCE, 1 = SPEAKING
    private int tThreshold = 300; //in ms
    private int vThreshold = 150;

    //playback variables
    private float reverbStrength = 0.0f;

    //others
    boolean isRecording = false;
    boolean isKilled = false;
    AudioManager am = null;
    AudioRecord record = null;
    AudioTrack track = null;
    private int audioSource = MediaRecorder.AudioSource.MIC;
    private int samplingRate = FREQUENCY; /* in Hz*/
    private int channelConfig = AudioFormat.CHANNEL_IN_MONO;
    private int audioFormat = AudioFormat.ENCODING_PCM_16BIT;
    private int bufferSize = AudioRecord.getMinBufferSize(samplingRate, channelConfig, audioFormat);
    private int sampleNumBits = 16;
    private int numChannels = 1;
    private int totalWriteCount = 0;

    private Thread recordingThread = null ;

    private int lastActivityType = -1;
    private int lastActivityDuration = 0;
    public String getRecord(){
        //in JSON
        if (isRecording == false)
            return "";

        String result = "";
        result += "{\"status\":[";

        //start to put in activities
        for (int i=0; i<VADRecord.size(); i++)
        {
            if ((i==0) && (lastActivityType != -1)) //information about part of the first activity had already been picked by Unity
            {
                result+="{\"status\":"+VADRecord.get(i).getType()+", \"time\":"+(VADRecord.get(i).getTime()-lastActivityDuration)+"}";
            }
            else
            {
                result+="{\"status\":"+VADRecord.get(i).getType()+", \"time\":"+VADRecord.get(i).getTime()+"}";
            }

            result += ",";
        }

        //the current ongoing activity
        lastActivityType = sCurrent;
        lastActivityDuration = tCurrent;
        result += "{\"status\":"+lastActivityType+", \"time\":"+lastActivityDuration+"}";
        result += "]}";
        VADRecord.clear();
        return result;
    }


    public void initialize_recordNplayback(String filename, int newThreshold) {
        vThreshold = newThreshold;
        initialize_recordNplayback(filename);
    }


    public void ChangeVolumeControl()
    {
        setVolumeControlStream(AudioManager.STREAM_MUSIC);

        Log.i("MainActivity", "Tried to set volume control TO STREAM MUSIC.");
    }


   // public void startRecording(String filename){
   public void initialize_recordNplayback(String filename){
        bIsVRrecord = true;
        isKilled = false;

        lastnanosec = System.nanoTime();
        //replayData = new ArrayList();
        VADRecord = new ArrayList();
        am = (AudioManager) this.getSystemService(Context.AUDIO_SERVICE);
        am.setMode(AudioManager.STREAM_MUSIC);
        //am.setSpeakerphoneOn(false);
        filepath = filename;
        if (filepath.length()==0){
            filepath = Environment.getExternalStorageDirectory().getPath() +"/record.pcm";
        }
        Log.i("MainActivity", "Will write file to '"+filepath+"'");

        //create a file to record the voice

        try { outputStream = new FileOutputStream(filepath, true); } //append if file exist
        catch (FileNotFoundException e) {e.printStackTrace();}

        this.setVolumeControlStream(AudioManager.STREAM_MUSIC);

        //isRecording = true;

        (recordingThread =  new Thread()
        {
            @Override
            public void run()
            {
                if (isKilled){

                }else{
                    recordAndPlay();
                }

            }
        }).start();

        initRecordAndTrack();
        startRecordAndPlay();
    }


    private void recordAndPlay() //this is on second thread
    {
        int bufferSizeInBytes = AudioRecord.getMinBufferSize(FREQUENCY, AudioFormat.CHANNEL_IN_MONO, AudioFormat.ENCODING_PCM_16BIT);

        byte byteData[] = new byte[bufferSizeInBytes];
        am.setMode(AudioManager.MODE_IN_COMMUNICATION);

        Log.i("MainActivity", "Recording thread start");

        while (true)
        {
            if (isKilled)
                break;

            if (isRecording) {
                int bufferReadByte = record.read(byteData, 0, bufferSizeInBytes);

                //write to file
                try {outputStream.write(byteData, 0, bufferReadByte);}
                catch (IOException e) {e.printStackTrace();}
                totalWriteCount += bufferReadByte;

                //write to playback track
                track.write(byteData, 0, bufferReadByte);

                //byte > short array for VAD
                int bufferReadShort = bufferReadByte/2;
                short[] shortArray = new short[bufferReadShort+1];
                if (bufferReadByte == 0)
                    continue;

                int j=0;
                for (j=0; j<bufferReadByte; j+=2)
                {
                    ByteBuffer bb = ByteBuffer.allocate(2);
                    bb.order(ByteOrder.LITTLE_ENDIAN);
                    bb.put(byteData[j]);
                    bb.put(byteData[j + 1]);
                    short shortVal = bb.getShort(0);
                    shortArray[j/2] = shortVal;
                }
                shortArray[j/2] = '\0';

                //vad: get average amplifier and compare with threshold
                int sumSample = 0;
                int countSample = 0; // = 0;

                for (int i = 0; i < bufferReadShort; i++)
                {
                    sumSample+=Math.abs((int)shortArray[i]);
                    countSample+=1;
                }

                //get time elapsed
                int elapsed = (int)((System.nanoTime() - lastnanosec) / 1000000); //in ms

                //judgment start
                if (countSample == 0) //prevent divided by 0
                    continue;
                int sNew = 0; //status of new sample, status = speak or not speak
                int avgAmplifier = sumSample / countSample;

                /*if (avgAmplifier >= 2*vThreshold)
                {
                    sNew = 2;
                }
                else */
                if (avgAmplifier >= vThreshold)
                    sNew = 1;
                else
                    sNew = 0;

                if (sNew == sCurrent)
                {
                    //if (tOpposite!=0)
                    //    Log.i("MainActivity", "Ignored a "+tOpposite+" event that is not "+sNew);
                    tOpposite = 0;
                    tCurrent += elapsed;
                }
                else //sNew!=sCurrent
                {
                    tOpposite += elapsed;
                    if (tOpposite >= tThreshold) { //status switch
                        //record the last status
                        Log.i("MainActivity", "Status change! "+sCurrent+" to "+sNew+" (last status lasts "+tCurrent+"ms)");
                        VADRecord.add(new VoiceActivityRecord(tCurrent, sCurrent));

                        tCurrent = tOpposite;
                        sCurrent = sNew;
                        tOpposite = 0;
                    }
                }

                lastnanosec = System.nanoTime();
                sumSample = 0;
                countSample = 0;
            }
        }
    }

    private void startRecordAndPlay() {
        record.startRecording();
        track.play();
        isRecording = true;
    }

    private void stopRecordAndPlay() {
        record.stop();
        track.pause();
        isRecording = false;
        bIsVRrecord = false;
    }

    //public String prepareReplay() {
    public void prepareReplay() {
        Log.i("MainActivity", "Stopping Recording ~~");

        bIsVRrecord = false;
        isRecording = false;
        isKilled = true;
        try {
            recordingThread.join();
        }
        catch (InterruptedException e)
        {
            e.printStackTrace();
        }

        record.stop();
        track.stop();
        record.release();
        track.release();

        try {outputStream.close();}
        catch (IOException e) {e.printStackTrace();}

        //filepath = Environment.getExternalStorageDirectory().getPath();

        Log.i("MainActivity", "Stopped ~~");
    }

    private void initRecordAndTrack(){

        //Log.i("MainActivity", "initRecordAndTrack");
        int min = AudioRecord.getMinBufferSize(FREQUENCY, AudioFormat.CHANNEL_IN_MONO, AudioFormat.ENCODING_PCM_16BIT);
        // record = new AudioRecord(MediaRecorder.AudioSource.VOICE_COMMUNICATION, FREQUENCY, AudioFormat.CHANNEL_IN_MONO, AudioFormat.ENCODING_PCM_16BIT, min);
        record = new AudioRecord(audioSource, samplingRate, channelConfig, audioFormat, bufferSize);

        int maxJitter = AudioTrack.getMinBufferSize(FREQUENCY, AudioFormat.CHANNEL_OUT_MONO, AudioFormat.ENCODING_PCM_16BIT);
        track = new AudioTrack(AudioManager.STREAM_MUSIC, FREQUENCY, AudioFormat.CHANNEL_OUT_MONO, AudioFormat.ENCODING_PCM_16BIT, maxJitter,
                AudioTrack.MODE_STREAM);
    }

}
