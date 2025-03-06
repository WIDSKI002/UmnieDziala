Clock’s Ticking
Dwu-osobowa gra coop. Wymaga drugiego gracza.

Opis gry
Build gry jest w folderze “Build” w repozytorium pod nazwą umniedziala.exe
 1.1 Ogólny opis rozgrywki  
"Clock’s Ticking" to gra multiplayer, w której gracze muszą rozwiązywać zagadki pod presją czasu. Każda sekunda jest cenna, gdyż na graczy czai się zegarowy potwór, istota zamieszkująca wymiar do którego trafili gracze, przed którą gracze muszą uciekać. Jeśli gracz nie zdąży rozwiązać zagadek na czas rozgrywka zaczyna się od początku
1.2 Zaimplementowane mechaniki
Poruszanie postaci- Gracze mogą swobodnie eksplorować otoczenie, szukając rozwiązań dla zagadek. 
Interakcja z przedmiotami- Można podnosić i używać przedmioty, które mogą okazać się kluczowe w rozwiązaniu zagadek. 
Ekwipunek- Przedmioty można gromadzić w ekwipunku, co pozwala na późniejsze ich wykorzystanie. 
System zagadek- Gracze muszą współpracować i analizować wskazówki, aby zdążyć na czas rozwiązać wszystkie zagadki. 
Kooperacja- Do rozwiązania niektórych zagadek potrzebna jest praca zespołowa.  
Zegarowy potwór- Przeciwnik, który porusza się po mapie i goni gracza. Im mniej czasu zostaje, tym szybciej się porusza.
System cofania czasu- gra zapisuje stan co godzinę (czasu w grze) by  umożliwić cofnięcie się w czasie w celu naprawienia swoich błędów.
 
1.3 Tło fabularne
Dwójka naukowców poprzez eksperyment trafia do wymiaru z dziwną anomalią czasową- utkneli w pętli czasu trwającej sześć godzin. Poprzez wspólne działanie muszą wydostać się z pętli i tajemniczego wymiaru. Podczas swojej przeprawy są kierowani przez notatki pozostawione przez innych nieszczęśników, którzy trafili do tego wymiaru przed nimi- ich zaginionego kolegi z pracy i dziwnego jegomościa 
z dużym ego.




2. Sterowanie. 
Sterowanie w grze jest obsługiwane za pomocą klawiatury i myszy. Poruszanie postaci obsługują klawisze W A S D lub klawisze strzałek. Poruszanie kamery obsługiwane jest poprzez ruch myszy. Interakcja z elementami w świecie gry odbywa się po naciśnięciu klawisza E na klawiaturze. Użycie przedmiotów jest pod lewym przyciskiem myszy, a wyrzucenie ich odbywa się poprzez prawy przycisk myszy. Pod ESC mamy menu pauzy.
Zegar który jest bardziej szary/niebieskawy służy do cofania czasu i stanu gry. Aby zmieniać godzinę do której się cofamy (o ile jest dostępna czyli już minęła) trzymamy prawy przycisk myszy i ruszamy nim w górę i w dół.
Finalnie należy otworzyć drzwi do korytarza które znajdują się w wielkim zegarowym pokoju. Niektóre zagadki wymagają współpracy dwóch osób.
Potwór pojawia się od pewnego momentu tzn. po otworzeniu pewnych drzwi.
Znajduje się tam zagadka "Tik Tak" która polega na powtarzaniu sekwencji przycisków (mały przycisk z prawej oznacza odtworzenie sekwencji)
Są skrzynki do których trzeba znaleźć i dopasować odpowiednie kolory kabli by otworzyć drzwi. W jednym pokoju musimy z drugim graczem się nimi wymienić, jeden gracz wpisuje także w tym pokoju kod który może znaleźć dzięki kartkom.
Drugi gracz także znajduje część kodu.
W pokoju zręcznościowym znajduje się także przycisk otwierające jedne drzwi.
Generalnie znajdują się w grze przyciski zagadki i zadania które otworzą korytarz w którym znajdujemy zieloną miksturę która oznacza ucieczkę z pętli czasu i wygranie gry.

Jak się połączyć?
Jeden z graczy musi hostować serwer, czyli podaje swój nick oraz dowolny wolny port. Drugi gracz, aby połączyć się z serwerem, musi wpisać swój nick, adres IP serwera oraz port pierwszego gracza. Gdy gracze znajdują się w lobby, klikają „Ready” i czekają, aż gra się uruchomi.
Konieczne może być przekierowanie portów na routerze oraz wyłączenie firewalla. Serwer hostuje gracz na swoim sprzęcie bez żadnego relay’a.
Po dołączeniu/załadowaniu nie można ruszać się przez około 10 sekund.

3. W jaki sposób gra nawiązuje do tematu.  
W grze występuje system czasu. Gracze mają 6 godzin (czas w grze, nie prawdziwe godziny) aby wykonać wszystkie zadania (rozwiązać zagadki) i wydostać się z pętli czasu, w której są uwięzieni.
Napisaliśmy cały system cofania czasu, tak aby np. w sytuacji gdy jeden gracz zginie, drugi żywy gracz może cofnąć czas gdy jeszcze oboje żyli. Cofa to postęp, przedmioty, pozycje, potwora, czas itd.- w skrócie: stan gry.
Gdy czas się skończy (wybije szósta) gracze są cofani do północy czyli początkowego stanu gry, gdyż są uwięzieni w pętli czasowej i nie udało im się z niej wydostać.
Dodatkowo, potwór sam w sobie jest czasowym potworem- porusza się odtwarzając dźwięki tykania zegara. Potwór porusza się wraz z tykaniem zegara. Zegar tyka.
Całe miejsce jest również stosunkowo wypełnione zegarami, oraz główne założenie fabularne to dosłownie pętla czasowa.
4. Proces twórczy
Elementy gry zostały tak skonstruowane, aby nawiązywały do tematyki gry. 
Grafika nawiązuje do tematyki licznymi zegarami symbolizującymi upływający czas. 
Efekty dźwiękowe pozwalają usłyszeć tykanie zegara, co wpływa na lepsze odczuwanie przemijającego czasu. 

Większość assetów- modeli, tekstur i część dźwięków stworzyliśmy sami.
Modele graczy- Grzegorz Sygut,
Model potwora- Daniel Araźny,
Kilka dźwięków- Adrian Zaleśny,
Większość tekstur- drzwi, ściany, zegary, płytki, świeczki, guziki, sufity, itemy oraz ikony do UI- Piotr Zieliński
Problemy sprawiły synchronizacja rzeczy w sieci (multiplayer) zwłaszcza w kontekście przedmiotów, nie jest idealna.
Modele nie są do końca jak sobie to wyobrażaliśmy a sam wygląd ogólny gry również.
Trafiły się małe błędy na które zabrakło czasu by je naprawić.
Gra miała mieć również o wiele więcej zagadek jednak sam w sobie proces tworzenia systemów zajął najwięcej czasu. 



5. Linki do assetów osób trzecich, które zostały wykorzystane w projekcie. 
FishNet: Networking Evolved | Unity Asset Store 
https://assetstore.unity.com/packages/tools/network/fishnet-networking-evolved-207815
Dark Fantasy Kit [Lite] | 3D Fantasy | Unity Asset Store 
https://assetstore.unity.com/packages/3d/environments/fantasy/dark-fantasy-kit-lite-127925
Stylized Fire VFX | VFX Particles | Unity Asset Store
https://assetstore.unity.com/packages/vfx/particles/stylized-fire-vfx-199626
Potions, Coin And Box of Pandora Pack | 3D Props | Unity Asset Store
https://assetstore.unity.com/packages/3d/props/potions-coin-and-box-of-pandora-pack-71778
LuKaiX - Rewind Short.wav
https://freesound.org/people/LuKaiX/sounds/700527/
Producing_RayLite - Incorrect Buzzer
https://freesound.org/people/Producing_RayLite/sounds/700641/
Snowback - Old clock - 4 o'clock
https://freesound.org/people/Snowback/sounds/718923/
Mixamo - animacje lokomocji (bieganie chodzenie gracza itd.)
https://www.mixamo.com/
GGBotNet - Moonlit Flow Font
https://www.fontspace.com/moonlit-flow-font-f136344
lettertypestudio - Raela Grotesque Font
https://www.fontspace.com/raela-grotesque-font-f136531
KineticPlasma Fonts - Falling Sky
https://www.fontspace.com/falling-sky-font-f22358 
LukaCafuka - Mechanical clock ticking 2
https://freesound.org/people/LukaCafuka/sounds/784052/
StaticDeath_9 by Xiko
https://freesound.org/s/711119/ License: Attribution 4.0
Abstract pattern design designed by Freepik
https://www.freepik.com/free-vector/abstract-pattern-design_921169.htm





