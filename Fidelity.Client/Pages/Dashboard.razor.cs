using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Fidelity.Shared.DTOs;

namespace Fidelity.Client.Pages
{
    public partial class Dashboard
    {
        [Inject]
        public HttpClient Http { get; set; } = default!;

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        private string nomeCompleto = "";
        private string authToken = "";
        private string ruolo = "";

        // Tab state
        private string activeTab = "overview";

        private bool isLoading = false;
        private string ricercaCliente = "";
        private List<ClienteResponse> clientiTrovati = new();
        
        private List<ClienteResponse> clienti = new();
        private List<PuntoVenditaResponse> puntiVendita = new();
        private List<TransazioneResponse> transazioni = new();
        private List<CouponDTO> coupons = new();
        
        private int totaleClienti = 0;
        private int puntiTotali = 0;
        private int mediaClientiPerNegozio = 0;

        // Store management
        private bool mostraModaleNegozio = false;
        private bool mostraModaleEliminazione = false;
        private PuntoVenditaResponse? negozioInModifica = null;
        private PuntoVenditaResponse? negozioInEliminazione = null;
        private PuntoVenditaRequest negozioForm = new();
        private string messaggioErroreNegozio = "";
        private string messaggioErroreEliminazione = "";

        // Manager management
        private List<ResponsabileDetailResponse> responsabili = new();
        private bool mostraModaleResponsabile = false;
        private bool mostraModaleEliminazioneResponsabile = false;
        private ResponsabileDetailResponse? responsabileInModifica = null;
        private ResponsabileDetailResponse? responsabileInEliminazione = null;
        private ResponsabileRequest responsabileForm = new();
        private string messaggioErroreResponsabile = "";
        private string messaggioErroreEliminazioneResponsabile = "";

        // Coupon management
        private bool mostraModaleCoupon = false;
        private bool mostraModaleEliminazioneCoupon = false;
        private CouponDTO? couponInModifica = null;
        private CouponDTO? couponInEliminazione = null;
        private CouponRequest couponForm = new();
        private string messaggioErroreCoupon = "";
        private string messaggioErroreEliminazioneCoupon = "";

        protected override async Task OnInitializedAsync()
        {
            nomeCompleto = await JS.InvokeAsync<string>("localStorage.getItem", "nomeCompleto");
            authToken = await JS.InvokeAsync<string>("localStorage.getItem", "authToken");
            ruolo = await JS.InvokeAsync<string>("localStorage.getItem", "ruolo");

            if (string.IsNullOrEmpty(authToken))
            {
                Navigation.NavigateTo("/login-responsabile");
                return;
            }

            if (ruolo != "Admin")
            {
                Navigation.NavigateTo("/benvenuto-responsabile");
                return;
            }

            Http.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            await CaricaDati();
        }

        private async Task CaricaDati()
        {
            isLoading = true;
            try
            {
                // Carica punti vendita con statistiche
                puntiVendita = await Http.GetFromJsonAsync<List<PuntoVenditaResponse>>("api/PuntiVendita") ?? new();
                
                // Carica tutti i clienti
                clienti = await Http.GetFromJsonAsync<List<ClienteResponse>>("api/Clienti/mio-punto-vendita") ?? new();
                
                // Carica responsabili
                responsabili = await Http.GetFromJsonAsync<List<ResponsabileDetailResponse>>("api/Responsabili") ?? new();

                // Carica transazioni
                await RubricaTransazioni();

                // Carica coupons
                coupons = await Http.GetFromJsonAsync<List<CouponDTO>>("api/Coupons") ?? new();
                
                totaleClienti = clienti.Count;
                puntiTotali = clienti.Sum(c => c.PuntiTotali);
                mediaClientiPerNegozio = puntiVendita.Count > 0 ? totaleClienti / puntiVendita.Count : 0;
            }
            catch { }
            finally
            {
                isLoading = false;
            }
        }

        private async Task CercaCliente()
        {
            if (string.IsNullOrWhiteSpace(ricercaCliente) || ricercaCliente.Length < 3)
            {
                clientiTrovati = new();
                return;
            }

            try
            {
                clientiTrovati = await Http.GetFromJsonAsync<List<ClienteResponse>>($"api/Clienti/cerca?query={ricercaCliente}") ?? new();
            }
            catch
            {
                clientiTrovati = new();
            }
        }

        private async Task RubricaTransazioni()
        {
            try
            {
                transazioni = await Http.GetFromJsonAsync<List<TransazioneResponse>>("api/Transazioni/storico?limit=100") ?? new();
            }
            catch { }
        }

        // Store CRUD operations
        private void ApriModaleCreazione()
        {
            negozioInModifica = null;
            negozioForm = new PuntoVenditaRequest { Attivo = true };
            messaggioErroreNegozio = "";
            mostraModaleNegozio = true;
        }

        private void ApriModaleModifica(PuntoVenditaResponse negozio)
        {
            negozioInModifica = negozio;
            negozioForm = new PuntoVenditaRequest
            {
                Codice = negozio.Codice,
                Nome = negozio.Nome,
                Citta = negozio.Citta,
                Indirizzo = negozio.Indirizzo,
                Telefono = negozio.Telefono,
                Attivo = negozio.Attivo
            };
            messaggioErroreNegozio = "";
            mostraModaleNegozio = true;
        }

        private void ChiudiModaleNegozio()
        {
            mostraModaleNegozio = false;
            negozioInModifica = null;
            messaggioErroreNegozio = "";
        }

        private async Task SalvaNegozio()
        {
            isLoading = true;
            messaggioErroreNegozio = "";

            try
            {
                HttpResponseMessage response;
                
                if (negozioInModifica == null)
                {
                    // Crea nuovo
                    response = await Http.PostAsJsonAsync("api/PuntiVendita", negozioForm);
                }
                else
                {
                    // Aggiorna esistente
                    response = await Http.PutAsJsonAsync($"api/PuntiVendita/{negozioInModifica.Id}", negozioForm);
                }

                if (response.IsSuccessStatusCode)
                {
                    // Per evitare problemi con il parsing della risposta, ricarica semplicemente i dati
                    await Task.Delay(500); // Piccolo delay per assicurare che il DB sia aggiornato
                    await CaricaDati();
                    ChiudiModaleNegozio();
                }
                else
                {
                    try
                    {
                        var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        messaggioErroreNegozio = errorObj?["messaggio"] ?? "Errore durante il salvataggio del negozio.";
                    }
                    catch
                    {
                        messaggioErroreNegozio = "Errore durante il salvataggio del negozio.";
                    }
                }
            }
            catch (Exception ex)
            {
                messaggioErroreNegozio = $"Errore di connessione: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        private void ApriModaleEliminazione(PuntoVenditaResponse negozio)
        {
            negozioInEliminazione = negozio;
            messaggioErroreEliminazione = "";
            mostraModaleEliminazione = true;
        }

        private void ChiudiModaleEliminazione()
        {
            mostraModaleEliminazione = false;
            negozioInEliminazione = null;
            messaggioErroreEliminazione = "";
        }

        private async Task EliminaNegozio()
        {
            if (negozioInEliminazione == null) return;

            isLoading = true;
            messaggioErroreEliminazione = "";

            try
            {
                var response = await Http.DeleteAsync($"api/PuntiVendita/{negozioInEliminazione.Id}");

                if (response.IsSuccessStatusCode)
                {
                    await CaricaDati();
                    ChiudiModaleEliminazione();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    messaggioErroreEliminazione = "Impossibile eliminare il negozio. Potrebbe avere clienti o responsabili associati.";
                }
            }
            catch
            {
                messaggioErroreEliminazione = "Errore di connessione.";
            }
            finally
            {
                isLoading = false;
            }
        }

        // Manager CRUD operations
        private void ApriModaleCrezioneResponsabile()
        {
            responsabileInModifica = null;
            responsabileForm = new ResponsabileRequest { Attivo = true, RichiestaResetPassword = true };
            messaggioErroreResponsabile = "";
            mostraModaleResponsabile = true;
        }

        private void ApriModaleModificaResponsabile(ResponsabileDetailResponse responsabile)
        {
            responsabileInModifica = responsabile;
            responsabileForm = new ResponsabileRequest
            {
                Username = responsabile.Username,
                NomeCompleto = responsabile.NomeCompleto,
                Email = responsabile.Email,
                PuntiVenditaIds = responsabile.PuntiVendita.Select(pv => pv.Id).ToList(),
                RichiestaResetPassword = responsabile.RichiestaResetPassword,
                Attivo = responsabile.Attivo
            };
            messaggioErroreResponsabile = "";
            mostraModaleResponsabile = true;
        }

        private void ChiudiModaleResponsabile()
        {
            mostraModaleResponsabile = false;
            responsabileInModifica = null;
            messaggioErroreResponsabile = "";
        }

        private void TogglePuntoVendita(int pvId, bool isChecked)
        {
            if (isChecked)
            {
                if (!responsabileForm.PuntiVenditaIds.Contains(pvId))
                {
                    responsabileForm.PuntiVenditaIds.Add(pvId);
                }
            }
            else
            {
                responsabileForm.PuntiVenditaIds.Remove(pvId);
            }
        }

        private async Task SalvaResponsabile()
        {
            isLoading = true;
            messaggioErroreResponsabile = "";

            try
            {
                // Validate form
                if (string.IsNullOrWhiteSpace(responsabileForm.Username))
                {
                    messaggioErroreResponsabile = "Username obbligatorio";
                    return;
                }

                if (string.IsNullOrWhiteSpace(responsabileForm.NomeCompleto))
                {
                    messaggioErroreResponsabile = "Nome completo obbligatorio";
                    return;
                }

                if (string.IsNullOrWhiteSpace(responsabileForm.Email))
                {
                    messaggioErroreResponsabile = "Email obbligatoria";
                    return;
                }

                if (!responsabileForm.PuntiVenditaIds.Any())
                {
                    messaggioErroreResponsabile = "Seleziona almeno un punto vendita";
                    return;
                }

                HttpResponseMessage response;
                if (responsabileInModifica == null)
                {
                    // Create new
                    response = await Http.PostAsJsonAsync("api/Responsabili", responsabileForm);
                }
                else
                {
                    // Update existing
                    response = await Http.PutAsJsonAsync($"api/Responsabili/{responsabileInModifica.Id}", responsabileForm);
                }

                if (response.IsSuccessStatusCode)
                {
                    await Task.Delay(500);
                    await CaricaDati();
                    ChiudiModaleResponsabile();
                }
                else
                {
                    try
                    {
                        var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        messaggioErroreResponsabile = errorObj?["messaggio"] ?? "Errore durante la creazione del responsabile.";
                    }
                    catch
                    {
                        messaggioErroreResponsabile = "Errore durante la creazione del responsabile.";
                    }
                }
            }
            catch (Exception ex)
            {
                messaggioErroreResponsabile = $"Errore di connessione: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        private void ApriModaleEliminazioneResponsabile(ResponsabileDetailResponse responsabile)
        {
            responsabileInEliminazione = responsabile;
            messaggioErroreEliminazioneResponsabile = "";
            mostraModaleEliminazioneResponsabile = true;
        }

        private void ChiudiModaleEliminazioneResponsabile()
        {
            mostraModaleEliminazioneResponsabile = false;
            responsabileInEliminazione = null;
            messaggioErroreEliminazioneResponsabile = "";
        }

        private async Task EliminaResponsabile()
        {
            if (responsabileInEliminazione == null) return;

            isLoading = true;
            messaggioErroreEliminazioneResponsabile = "";

            try
            {
                var response = await Http.DeleteAsync($"api/Responsabili/{responsabileInEliminazione.Id}");

                if (response.IsSuccessStatusCode)
                {
                    await CaricaDati();
                    ChiudiModaleEliminazioneResponsabile();
                }
                else
                {
                    try
                    {
                        var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        messaggioErroreEliminazioneResponsabile = errorObj?["messaggio"] ?? "Impossibile eliminare il responsabile.";
                    }
                    catch
                    {
                        messaggioErroreEliminazioneResponsabile = "Impossibile eliminare il responsabile.";
                    }
                }
            }
            catch
            {
                messaggioErroreEliminazioneResponsabile = "Errore di connessione.";
            }
            finally
            {
                isLoading = false;
            }
        }

        // Coupon CRUD operations
        private void ApriModaleCreazioneCoupon()
        {
            couponInModifica = null;
            couponForm = new CouponRequest 
            { 
                Attivo = true,
                TipoSconto = "Percentuale",
                DataInizio = DateTime.Today,
                DataScadenza = DateTime.Today.AddDays(30),
                ValoreSconto = 10
            };
            messaggioErroreCoupon = "";
            mostraModaleCoupon = true;
        }

        private void ApriModaleModificaCoupon(CouponDTO coupon)
        {
            couponInModifica = coupon;
            couponForm = new CouponRequest
            {
                Codice = coupon.Codice,
                Titolo = coupon.Titolo,
                Descrizione = coupon.Descrizione,
                ValoreSconto = coupon.ValoreSconto,
                TipoSconto = coupon.TipoSconto,
                DataInizio = coupon.DataInizio,
                DataScadenza = coupon.DataScadenza,
                Attivo = coupon.Attivo
            };
            messaggioErroreCoupon = "";
            mostraModaleCoupon = true;
        }

        private void ChiudiModaleCoupon()
        {
            mostraModaleCoupon = false;
            couponInModifica = null;
            messaggioErroreCoupon = "";
        }

        private async Task SalvaCoupon()
        {
            isLoading = true;
            messaggioErroreCoupon = "";

            try
            {
                // Simple validation
                if (string.IsNullOrWhiteSpace(couponForm.Codice))
                {
                    messaggioErroreCoupon = "Il codice è obbligatorio.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(couponForm.Titolo))
                {
                    messaggioErroreCoupon = "Il titolo è obbligatorio.";
                    return;
                }
                if (couponForm.ValoreSconto <= 0)
                {
                    messaggioErroreCoupon = "Il valore deve essere maggiore di zero.";
                    return;
                }

                HttpResponseMessage response;
                if (couponInModifica == null)
                {
                    // Create
                    response = await Http.PostAsJsonAsync("api/Coupons", couponForm);
                }
                else
                {
                    // Update
                    response = await Http.PutAsJsonAsync($"api/Coupons/{couponInModifica.Id}", couponForm);
                }

                if (response.IsSuccessStatusCode)
                {
                    await Task.Delay(500);
                    await CaricaDati();
                    ChiudiModaleCoupon();
                }
                else
                {
                    try
                    {
                        var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        messaggioErroreCoupon = errorObj?["messaggio"] ?? "Errore durante il salvataggio del coupon.";
                    }
                    catch
                    {
                        messaggioErroreCoupon = "Errore durante il salvataggio del coupon.";
                    }
                }
            }
            catch (Exception ex)
            {
                messaggioErroreCoupon = $"Errore di connessione: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        private void ApriModaleEliminazioneCoupon(CouponDTO coupon)
        {
            couponInEliminazione = coupon;
            messaggioErroreEliminazioneCoupon = "";
            mostraModaleEliminazioneCoupon = true;
        }

        private void ChiudiModaleEliminazioneCoupon()
        {
            mostraModaleEliminazioneCoupon = false;
            couponInEliminazione = null;
            messaggioErroreEliminazioneCoupon = "";
        }

        private async Task EliminaCoupon()
        {
            if (couponInEliminazione == null) return;

            isLoading = true;
            messaggioErroreEliminazioneCoupon = "";

            try
            {
                var response = await Http.DeleteAsync($"api/Coupons/{couponInEliminazione.Id}");

                if (response.IsSuccessStatusCode)
                {
                    await CaricaDati();
                    ChiudiModaleEliminazioneCoupon();
                }
                else
                {
                    messaggioErroreEliminazioneCoupon = "Errore durante l'eliminazione del coupon.";
                }
            }
            catch
            {
                messaggioErroreEliminazioneCoupon = "Errore di connessione.";
            }
            finally
            {
                isLoading = false;
            }
        }
        private async Task Logout()
        {
            await JS.InvokeVoidAsync("localStorage.clear");
            Navigation.NavigateTo("/");
        }
    }
}
