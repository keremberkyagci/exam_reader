-- Supabase SQL Editor'de çalıştırılacak script

-- 1. Tabloyu oluşturma
CREATE TABLE IF NOT EXISTS public.exam_results (
    id SERIAL PRIMARY KEY,
    student_name VARCHAR(100) NOT NULL,
    score INTEGER NOT NULL CHECK (score >= 0 AND score <= 100),
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

-- 2. Güvenlik ve Yetkilendirme (Row Level Security) (İsteğe bağlı, Dapper için şart değil)
-- Tablonun herkes tarafından okunup yazılmasına izin vermek istersen (Geliştirme aşaması):
ALTER TABLE public.exam_results ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Enable read access for all users"
ON public.exam_results FOR SELECT
USING (true);

CREATE POLICY "Enable insert access for all users"
ON public.exam_results FOR INSERT
WITH CHECK (true);

-- Eğer Supabase Studio'da tabloyu göremiyorsan sayfayı yenile.
