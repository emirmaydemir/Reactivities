namespace Domain
{
    public class Photo
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; } //Kullanıcının ana fotoğrafı mı katılımcı görselinde hangi fotoyu gösterelim diye olan bir özelliktir.
    }
}