import { User } from "./user";

export interface Profile {
  userName: string; //ActivityDto sınıfında UserName olarak yazıyor neden burada userName yazıyoruz dersen çünkü json formatında otomatik olarak label case yani ilk harfi küçük yapılyıor iki sınıfın eşleşmesi için buradakini küçük yazıyorum oysa ki diğer sınıfta büyük harfle yazılmış ama postmanda istek atınca json gövdesinde görebilirsin UserName yerine userName yazacaktır.
  displayName: string;
  image?: string;
  bio?: string;
}

export class Profile implements Profile {
  constructor(user: User) {
    this.userName = user.username;
    this.displayName = user.displayName;
    this.image = user.image;
  }
}
