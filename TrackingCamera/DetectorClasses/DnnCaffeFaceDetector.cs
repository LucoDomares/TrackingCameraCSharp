using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using TrackingCamera.BaseDetectorClasses;
using TrackingCamera.Helpers;

namespace TrackingCamera.DetectorClasses
{
	public class DnnCaffeFaceDetector : BaseDetectorFace
	{
		private String DetectorPath { get; set; }

		private String ProtoFile { get; set; }

		private String DetectorModelFile { get; set; }

		private String EmbeddingModelFile { get; set; }

		private String RecogniserModelFile { get; set; }

		private String LabelEncoderFile { get; set; }

		private String ProtoPath { get; set; }

		private String ModelPath { get; set; }

		private new Net Detector { get; set; }

		private String EmbedderPath { get; set; }

		private new Net Embedder { get; set; }

		//private Net Regogniser { get; set; }

		//private Net LabelEncoder { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public DnnCaffeFaceDetector(Double MinConfidence, String detectorPath, string protoFile, string detectorModelFile, string embeddingModelFile,
				string recogniserModelFile, string labelEncoderFile) : base(MinConfidence)
		{
			this.DetectorPath = detectorPath;
			this.ProtoFile = protoFile;
			this.DetectorModelFile = detectorModelFile;
			this.EmbeddingModelFile = embeddingModelFile;
			this.RecogniserModelFile = recogniserModelFile;
			this.LabelEncoderFile = labelEncoderFile;

			try
			{
				// read in the cafe DNN from disk
				this.ProtoPath = System.IO.Path.Join(this.DetectorPath, this.ProtoFile);
				this.ModelPath = System.IO.Path.Join(this.DetectorPath, this.DetectorModelFile);
				this.Detector = CvDnn.ReadNetFromCaffe(this.ProtoPath, this.ModelPath);

				// load the serialised face embedding model from disk.
				//this.EmbedderPath = System.IO.Path.Join(this.DetectorPath, this.EmbeddingModelFile);
				//this.Embedder = CvDnn.ReadNetFromTorch(this.EmbedderPath);

				// todo: load facial recognition model and label encoder.
				//this.Regogniser = pickle.loads(open(self._recogniser_model_file, "rb").read())
				//this.LabelEncoder = pickle.loads(open(self._label_encoder_file, "rb").read())
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
			}


		}

		protected override Mat GetFaces(Mat frame, out FacesList facesList)
		{
			// debug - replace video stream with still image for testing
			//var file = "C:\\Users\\Lamby\\Documents\\Visual Studio 2017\\Projects\\TrackingCamera\\TrackingCamera\\DetectorClasses\\ModelDetectorVGG_VOC0712Plus\\bali-crop.jpg";
			string file = "C:\\Users\\Lamby\\Desktop\\fd-acc-result3-e1539872783684.jpg";
			frame = Cv2.ImRead(file);

			Mat imageBlob = CvDnn.BlobFromImage(frame, 1.0, new Size(300, 300),
										  new Scalar(104.0, 177.0, 123.0), false, false);

			this.Detector.SetInput(imageBlob, "data");
			Mat detections = this.Detector.Forward("detection_out");

			//reshape from [1,1,200,7] to [200,7]
			Mat detectionMat = detections.Reshape(1, detections.Size(2));

			// debug
			//GetFaceBestConfidence(detections, out int faceId, out double faceProbability);

			if (detectionMat.Rows <= 0) // 
			{
				facesList = new FacesList();
				return null;
			}
			else
			{
				facesList = new FacesList();
				Scalar rgbColour = new Scalar(0, 255, 255);

				for (int i = 0; i < detectionMat.Rows; i++)
				{
					var confidence = detectionMat.At<float>(i, 2);

					if (confidence > this.MinConfidence)
					{
						int X1 = (int)(detectionMat.At<float>(i, 3) * frame.Width);   //detectionMat.At<int> returns 0 with this floating point caffe model?
						int Y1 = (int)(detectionMat.At<float>(i, 4) * frame.Height);
						int X2 = (int)(detectionMat.At<float>(i, 5) * frame.Width);
						int Y2 = (int)(detectionMat.At<float>(i, 6) * frame.Height);
						
						frame.Rectangle(new Point(X1, Y1), new Point(X2, Y2), rgbColour, 2, OpenCvSharp.LineTypes.Link4);
						string faceText = String.Format("{0:P2}", confidence);
						Cv2.PutText(frame, faceText, new Point(X1, Y2 + 9), HersheyFonts.HersheyComplex, 0.3, rgbColour);

						var faceMat = frame[new Rect(X1, Y1, X2 - X1, Y2 - Y1)];
						facesList.Add(new Face(faceMat, new Point(X1, Y1), new Point(X2, Y2), confidence));
						
						// Debug
						//Cv2.ImShow("Detected Face", faceMat);
						//Cv2.WaitKey(1);
					}
				}

				return frame;
			}			
		}

		// todo: use this for getting highest probablility confidence face?
		/// <summary>
		/// Find best face for the blob (i. e. face with maximal probability)
		/// </summary>
		/// <param name="probBlob">the collection of faces</param>
		/// <param name="FaceId">the Id of the face with best probability.</param>
		/// <param name="FaceProb">the blob of the face matching FaceId</param>
		private static void GetFaceBestConfidence(Mat probBlob, out int FaceId, out double FaceProb)
		{
			// reshape the blob to 1x1000 matrix
			using (Mat probMat = probBlob.Reshape(1, 1))
			{
				Cv2.MinMaxLoc(probMat, out _, out FaceProb, out _, out var FaceNumber);
				FaceId = FaceNumber.X;
			}	
		}
	}
}
